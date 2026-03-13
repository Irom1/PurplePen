using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // A printing target that prints to a PDF file.
    public class PdfPrintTarget : IPrintingTarget
    {
        private readonly string pathName;
        private readonly bool cmykMode;

        private PdfWriter pdfWriter;

        public PdfPrintTarget(string pathName, bool cmykMode)
        {
            this.pathName = pathName;
            this.cmykMode = cmykMode;
        }

        public void StartPrinting(string documentTitle, int pageCount)
        {
            pdfWriter = new PdfWriter(documentTitle, cmykMode);
        }

        public void EndPrinting()
        {
            pdfWriter.Save(pathName);
        }

        public void PrintPage(int pageNumber, PrintingPaperSize paperSize, Action<IGraphicsTarget> drawPage)
        {
            using (IGraphicsTarget grTarget = pdfWriter.BeginPage(paperSize.SizeInInches)) {
                drawPage(grTarget);
            }
        }
    }
}
