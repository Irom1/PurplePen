using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AvUtil
{
    // Encapsulates something that has a size, can draw itself, typically slowly,
    // and can indicate when it needs to be redrawn. Usually this will be adapted using
    // a CachedDrawing, which adapts an IAsyncSkiaDrawing to an IAvaloniaDrawing by caching
    // the result of the drawing function in a bitmap.

    public interface IAsyncSkiaDrawing
    {
        // Bounds of the entire drawing. Nothing outside this bounds will be cached.
        public SKRect Bounds { get; }

        // Draw the given rectangle, at it's natural coordinates, to the canvas.
        // Its expected that this create a background task to do the drawing.
        public Task DrawAsync(SKCanvas canvas, SKRect rectToDraw, SKSizeI pixelSize, CancellationToken cancelToken);

        // Raised when drawing has changed.
        public event EventHandler? DrawingChanged;
    }
}
