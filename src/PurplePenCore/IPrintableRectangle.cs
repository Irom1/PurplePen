using PurplePen.Graphics2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // The interface that the RectanglePositioner class uses to position and draw pages of rectangles.
    public interface IPrintableRectangle
    {
        // Number of boxes in the description.
        Size Boxes { get; }

        // Draw all or part of the description.
        void Draw(IGraphicsTarget grTarget, float x, float y, int startLine, int countLines);
    }

}
