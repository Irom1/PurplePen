using PurplePen.Graphics2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // Represents a paper size for printing.  
    // The Name may be "Custom", for a custom size, or it may be a standard name such as "Letter" or "A4".  
    // Generally you can just look at the SizeInInches or SizeInHundreths to get the size of the paper, and ignore the 
    // Landscape field.  The Landscape field is just a convenience to indicate whether the width is greater than the height.
    public class PrintingPaperSize
    {
        public string Name { get; private set; }
        public bool Landscape { get { return SizeInInches.Width > SizeInInches.Height; } }
        public SizeF SizeInInches { get; private set; }  // in inches
        public SizeF SizeInHundreths {
            get {
                return new SizeF(SizeInInches.Width * 100F, SizeInInches.Height * 100F);
            }
        }

        public PrintingPaperSize(string name, float widthInHundreths, float heightInHundreths)
        {
            Name = name;
            SizeInInches = new SizeF(widthInHundreths / 100F, heightInHundreths / 100F);
        }

        // Sets
        public PrintingPaperSize(float widthInHundreths, float heightInHundreths)
        {
            Name = "Custom";
            SizeInInches = new SizeF(widthInHundreths / 100F, heightInHundreths / 100F);
        }

        // Set a paper size to be the same as an original paper size, but with the orientation (landscape vs portrait) changed if needed.
        public PrintingPaperSize(bool landscape, PrintingPaperSize original)
        {
            Name = original.Name;
            if (landscape == original.Landscape) {
                SizeInInches = original.SizeInInches;
            }
            else {
                SizeInInches = new SizeF(original.SizeInInches.Height, original.SizeInInches.Width);
            }
        }

        // Flip landscape.
        public PrintingPaperSize Flip()
        {
            return new PrintingPaperSize(!Landscape, this);
        }
    }

    // Represents the margins for printing.
    public class PrintingMarginSize
    {
        public float LeftInInches { get; private set; }
        public float TopInInches { get; private set; }
        public float RightInInches { get; private set; }
        public float BottomInInches { get; private set; }

        public float LeftInHundreths { get { return LeftInInches * 100F; } }
        public float TopInHundreths { get { return TopInInches * 100F; } }
        public float RightInHundreths { get { return RightInInches * 100F; } }
        public float BottomInHundreths { get { return BottomInInches * 100F; } }

        public PrintingMarginSize(float leftInHundreths, float topInHundreths, float rightInHundreths, float bottomInHundreths)
        {
            LeftInInches = leftInHundreths / 100F;
            TopInInches = topInHundreths / 100F;
            RightInInches = rightInHundreths / 100F;
            BottomInInches = bottomInHundreths / 100F;
        }

        public PrintingMarginSize(float marginInHundreths)
        {
            LeftInInches = marginInHundreths / 100F;
            TopInInches = marginInHundreths / 100F;
            RightInInches = marginInHundreths / 100F;
            BottomInInches = marginInHundreths / 100F;
        }

        public PrintingMarginSize RotateLeft()
        {
            return new PrintingMarginSize(TopInHundreths, RightInHundreths, BottomInHundreths, LeftInHundreths);
        }

        public PrintingMarginSize RotateRight()
        {
            return new PrintingMarginSize(BottomInHundreths, LeftInHundreths, TopInHundreths, RightInHundreths);
        }

    }

    // Interface that is implemented to print pages onto "paper". Also used for print preview, and 
    // for creating PDFs.
    public interface IPrintingTarget
    {
        // Called at the beginning of printing.  Number of pages to print is given.
        void StartPrinting(int pageCount);

        // Called at the end of printing.
        void EndPrinting();

        // Called to print a page.  The page number is given (starting at 1).  
        // Call the "drawPage" function to draw the page; the IGraphicsTarget passed to the drawPage function is set up so that (0,0) is the top left corner of the page
        // (not the top-left corner of the printable area; not the top-left corner with the margins),
        // and the units are in hundredths of an inch. It should be already cleared to white if needed.
        void PrintPage(int pageNumber, PrintingPaperSize paperSize, Action<IGraphicsTarget> drawPage);
    }

    // Abstract class that is implemented by things that can be printed. For example, the courses, control descriptions,
    // punch cards, reports, etc.
    public interface IPrintable
    {
        // Layout the pages, and return the total number of papers. For layout, you typically only need to pay attendtion to the defaultPrintableAreaInInches,
        // which takes into account the paper size and margin size.
        int LayoutPages(PrintingPaperSize defaultPaperSize, PrintingMarginSize defaultMarginSize, SizeF defaultPrintableAreaInInches);

        // Get the paper size for a particular page number. This is generally used to set the orientation to
        // landscape or portrait. The defaultPaperSize and defaultMarginSize are the same as passed to LayoutPages.
        // It is called before DrawPage.
        PrintingPaperSize GetPagePaperSize(int pageNumber, PrintingPaperSize defaultPaperSize, PrintingMarginSize defaultMarginSize);

        // Draw a page onto a graphics target. The graphics target is set up so that (0,0) is the top left corner of the area to print,
        // taking into account the margins, and the units are in hundredths of an inch. It should be already cleared to white if needed.
        int DrawPage(IGraphicsTarget grTarget, int pageNumber);

        // Called after printing is complete or if it was cancelled.
        void PrintingComplete();
    }


    // Standard sizes.
    public static class PrintingStandards
    {
        // Standard paper sizes, in hundredths of an inch.  The first 10 are metric sizes, the last 3 are English sizes.
        public static PrintingPaperSize[] StandardPaperSizes = {
            new PrintingPaperSize("A2", 1654, 2339),
            new PrintingPaperSize("A3", 1169, 1654),
            new PrintingPaperSize("A4", 827, 1169),
            new PrintingPaperSize("A5", 583, 827),
            new PrintingPaperSize("A6", 413, 583),
            new PrintingPaperSize("B2", 1969, 2783),
            new PrintingPaperSize("B3", 1390, 1969),
            new PrintingPaperSize("B4", 984, 1390),
            new PrintingPaperSize("B5", 693, 984),
            new PrintingPaperSize("B6", 492, 693),
            new PrintingPaperSize("Letter", 850, 1100),
            new PrintingPaperSize("Legal", 850, 1400),
            new PrintingPaperSize("Tabloid", 1100, 1700)
        };

        public const int FirstMetricPaperSizeIndex = 0;
        public const int FirstEnglishPaperSizeIndex = 10;
        public const int DefaultEnglighPaperSizeIndex = 10;
        public const int DefaultMetricPaperSizeindex = 2;

        public const int DefaultEnglishMarginInHundreths = 25;  // 1/4 of a inch.
        public const int DefaultMetricMarginInHundreths = 28; // 7mm
    }
}
