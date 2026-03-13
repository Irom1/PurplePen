using PurplePen.Graphics2D;
using System;
using System.Drawing;
using Matrix = PurplePen.Graphics2D.Matrix;

namespace PurplePen
{
    // Connects an IPrintable to an IPrintingTarget to perform printing.
    // Handles layout, per-page paper size overrides, and margin translation
    // so that the IPrintable draws relative to the printable area.
    public class PrintManager
    {
        private IPrintingTarget printingTarget;
        private IPrintable printable;
        private string documentTitle;
        private PrintingPaperSize defaultPaperSize;
        private PrintingMarginSize defaultMarginSize;

        // Create a PrintManager that prints the given printable onto the given target.
        public PrintManager(string documentTitle, IPrintingTarget printingTarget, IPrintable printable)
        {
            this.documentTitle = documentTitle;
            this.printingTarget = printingTarget;
            this.printable = printable;
        }

        // Set the default paper size and margin size used for layout and printing.
        public void SetDefaultPaperSize(PrintingPaperSize paperSize, PrintingMarginSize marginSize)
        {
            this.defaultPaperSize = paperSize;
            this.defaultMarginSize = marginSize;
        }

        // Perform the printing. Lays out the pages using the default paper and margin sizes,
        // then prints each page, translating the coordinate system by the margins so that
        // the IPrintable draws at (0,0) relative to the printable area.
        public void DoPrinting()
        {
            if (defaultPaperSize == null || defaultMarginSize == null) {
                throw new InvalidOperationException("Default paper size and margin size must be set before printing.");
            }

            SizeF defaultPrintableAreaInInches = new SizeF(
                defaultPaperSize.SizeInInches.Width - defaultMarginSize.LeftInInches - defaultMarginSize.RightInInches,
                defaultPaperSize.SizeInInches.Height - defaultMarginSize.TopInInches - defaultMarginSize.BottomInInches);

            int pageCount = printable.LayoutPages(defaultPaperSize, defaultMarginSize, defaultPrintableAreaInInches);

            printingTarget.StartPrinting(documentTitle, pageCount);

            try {
                for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++) {
                    PrintingPaperSize pagePaperSize = printable.GetPagePaperSize(pageNumber);
                    int currentPageNumber = pageNumber; // Must capture pageNumber for the closure below.

                    printingTarget.PrintPage(pageNumber, pagePaperSize, grTarget => {

                        // Translate so (0,0) is at the top-left of the printable area (inside the margins).
                        // The IPrintingTarget provides coordinates in hundredths of an inch from the page top-left.
                        Matrix translateTransform = new Matrix();
                        translateTransform.Translate(defaultMarginSize.LeftInHundreths, defaultMarginSize.TopInHundreths);
                        grTarget.PushTransform(translateTransform);
                        try {
                            printable.DrawPage(grTarget, currentPageNumber);
                        }
                        finally {
                            grTarget.PopTransform();
                        }
                    });
                }
            }
            finally {
                printingTarget.EndPrinting();
                printable.PrintingComplete();
            }
        }
    }
}
