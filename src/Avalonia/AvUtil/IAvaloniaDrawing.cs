using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AvUtil
{
    // Encapsulates something that has a size, can draw itself, and can indicate when it needs to be redrawn.
    public interface IAvaloniaDrawing
    {
        // Bounds of the entire drawing. Nothing outside this bounds will be cached.
        public Rect Bounds { get; }

        // Draw to the drawing context.
        void Draw(DrawingContext drawingContext, Rect rectToDraw, PixelSize pixelSize);

        // This event is raised when the drawing changes and needs to be redrawn.
        // It is up to the consumer to call Draw again.
        public event EventHandler? DrawingChanged;
    }
}
