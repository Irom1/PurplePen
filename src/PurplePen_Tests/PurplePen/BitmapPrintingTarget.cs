using PdfSharp;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen_Tests.PurplePen
{
    // A printing target that prints to a list of bitmaps, one per page, for testing purposes.
    internal class BitmapPrintingTarget : IPrintingTarget
    {
        const float Dpi = 200F;   // using 200 dpi.

        int currentPage;  // pages start at 1.
        List<Bitmap> bitmaps = new List<Bitmap>();
        string documentTitle;

        public Bitmap[] Bitmaps => bitmaps.ToArray();

        public string DocumentTitle => documentTitle;

        public void StartPrinting(string documentTitle, int pageCount)
        {
            currentPage = 1;
            this.documentTitle = documentTitle;
        }

        public float GetPrinterDpi()
        {
            return Dpi;
        }

        public void PrintPage(int pageNumber, PrintingPaperSize paperSize, Action<IGraphicsTarget> drawPage)
        {
            Debug.Assert(pageNumber == currentPage, "Page numbers must start at 1 and be printed in order.");

            Bitmap bm = new Bitmap((int) Math.Round(paperSize.SizeInHundreths.Width * 2), (int) Math.Round(paperSize.SizeInHundreths.Height * 2), GDIPlus_GraphicsTarget.NonAlphaPixelFormat);
            bm.SetResolution(Dpi, Dpi);           

            using (Graphics g = Graphics.FromImage(bm)) {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.ScaleTransform(2, 2);  // Scaling must be set in 1/100 of an inch.

                using (IGraphicsTarget grTarget = new GDIPlus_GraphicsTarget(g))
                    drawPage(new GDIPlus_GraphicsTarget(g));
            }

            bitmaps.Add(bm);

            ++currentPage;
        }

        public void EndPrinting()
        {
        }
    }
}
