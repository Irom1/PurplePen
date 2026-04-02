using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AvUtil
{
    // Encapsulates something that has a size, can draw itself (potentially slowly),
    // and can indicate when it needs to be redrawn. Implementations must be safe to call
    // from a background thread, but should not create their own Task — the caller
    // (CachedDrawing) manages the threading. Usually this will be adapted using
    // a CachedDrawing, which adapts an IThreadsafeSkiaDrawing to an IAvaloniaDrawing
    // by caching the result of the drawing function in a bitmap.

    public interface IThreadsafeSkiaDrawing
    {
        // Bounds of the entire drawing. Nothing outside this bounds will be cached.
        public SKRect Bounds { get; }

        // Draw the given rectangle, at its natural coordinates, to the canvas.
        // This method may be called from a background thread. Implementations must
        // be thread-safe but should draw synchronously (do not use Task.Run internally).
        public void ThreadsafeDraw(SKCanvas canvas, SKRect rectToDraw, SKSizeI pixelSize, CancellationToken cancelToken);

        // Raised when drawing has changed.
        public event EventHandler? DrawingChanged;
    }
}
