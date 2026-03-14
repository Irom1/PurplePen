using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen.Graphics2D
{
    // Interface that abstracts writing to PDF Files.
    public interface IPdfWriter
    {
        // Create a PDF document writer to write a PDF.
        IPdfDocumentWriter CreateDocument(string fileName, string title, bool cmykMode);

        // Test whether we can read a page of a PDF. Returns true if we can, false if not.
        bool CanReadPdfPage(string pdfImport, int pageImport);
    }


    // Interface that abstracts writing to a single PDF file, using IGraphicsTarget
    // as the graphics interface for drawing.
    public interface IPdfDocumentWriter
    {
        // Begin writing to a blank page.
        IGraphicsTarget BeginPage(SizeF pageSizeInInches);

        // Begin writing a page that is initialized with the contents of an existing PDF page.
        // The pdfImport is the file name of the PDF to import from, and pageImport is the page number (0-based) to import from that file.
        // The page size is determined from the imported page.
        IGraphicsTarget BeginCopiedPage(string pdfImport, int pageImport);

        // Begin writing a page that is initialized with the partial contents of an existing PDF page.
        // The pdfImport is the file name of the PDF to import from, and pageImport is the page number (0-based) to import from that file.
        // partialSourcePageInInches is a rectangle in the coordinate space of the imported PDF page that specifies the portion of the page to import,
        // and destinationInInches is a rectangle in the coordinate space of the new PDF page that specifies where to place the imported portion.
        IGraphicsTarget BeginCopiedPartialPage(string pdfImport, int pageImport, SizeF pageSizeInInches, RectangleF partialSourcePageInInches, RectangleF destinationInInches);

        // Finish writing to the current page. After calling EndPage, the caller should dispose the IGraphicsTarget returned by BeginPage or BeginCopiedPage.
        void EndPage(IGraphicsTarget graphicsTarget);

        // Save the PDF document to the file specified in CreateDocument.
        void Save();
    }
}
