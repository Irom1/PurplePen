using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
using System.Text;

namespace AvUtil
{
    // A class that tracks a WriteableBitmap and ensures it is disposed properly.
    // If the bitmap is not disposed, it logs a warning with the stack trace of where it was allocated.
    //
    // Note: it is OK to Dispose of a WriteableBitmapTracker after it is used to draw to a DrawingContext.
    // However: if you draw AGAIN to it, then that drawing may be picked up by the render thread (although it
    // will respect Lock calls). So while it is tempting to reuse WritableBitmaps to avoid allocating new ones, 
    // it can cause drawing artifacts, so generally you shouldn't, unless you are reusing it to draw the exact
    // same portion of the same control (like SkiaDrawingView does).
    public class WriteableBitmapTracker : IDisposable
    {
        public WriteableBitmap Bitmap { get; }
        private bool _isDisposed;
        private readonly string _allocationStackTrace;

        // Allocated a new WriteableBitmap of the given pixel size.
        public WriteableBitmapTracker(PixelSize pixelSize)
        {
            Bitmap = new WriteableBitmap(
                pixelSize,
                new Vector(96, 96),
                SKImageInfo.PlatformColorType.ToPixelFormat(),
                AlphaFormat.Premul);

            _allocationStackTrace = Environment.StackTrace;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            Bitmap.Dispose();
            _isDisposed = true;

            // Tells the GC: "I cleaned up properly, don't run the finalizer."
            GC.SuppressFinalize(this);
        }

        // The Finalizer (Destructor)
        ~WriteableBitmapTracker()
        {
            if (!_isDisposed) {
                // If the GC is running this, it means Dispose() was NEVER called.
                // You can log this, write to a file, or trigger a Debugger.Break()
                Console.WriteLine("MEMORY LEAK DETECTED: WriteableBitmap was not disposed!");
                Console.WriteLine($"Allocated at: {_allocationStackTrace}");
                Debug.WriteLine("MEMORY LEAK DETECTED: WriteableBitmap was not disposed!");
                Debug.WriteLine($"Allocated at: {_allocationStackTrace}");
            }
        }
    }
}
