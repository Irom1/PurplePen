using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;



namespace AvUtil
{
    // Utility methods for drawing to Avalonia WriteableBitmaps using SkiaSharp.
    public static class SkiaWriteableBitmapUtil
    {
        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static WriteableBitmapTracker DrawToBitmap(PixelSize pixelSize, Action<SKCanvas> draw)
        {
            return DrawToBitmap(pixelSize, (canvas, _) => draw(canvas));
        }

        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static WriteableBitmapTracker DrawToBitmap(PixelSize pixelSize, Action<SKCanvas, CancellationToken> draw, CancellationToken token = default)
        {
            WriteableBitmapTracker bitmapTracker = new WriteableBitmapTracker(pixelSize);

            try {
                DrawToBitmap(bitmapTracker, draw, token);
            }
            catch {
                // If an exception occurs during drawing,
                // dispose the bitmap that we aren't going to return.
                bitmapTracker.Dispose();
                throw;
            }

            return bitmapTracker;
        }

        // Draw to an existing WriteableBitmap using the given drawing function.
        // Note: you need to be careful with WriteableBitmaps that you draw to AFTER you have drawn them to a drawing context.
        // The drawing context holds onto the WriteableBitmap, and will render it some time later. It is OK to Dispose of it, but if
        // you draw again to it, that later drawing might be rendered by the render thread.
        public static void DrawToBitmap(WriteableBitmapTracker bitmapTracker, Action<SKCanvas, CancellationToken> draw, CancellationToken token = default)
        {
            using (var framebuffer = bitmapTracker.Bitmap.Lock()) {
                var info = new SKImageInfo(
                    framebuffer.Size.Width,
                    framebuffer.Size.Height,
                    framebuffer.Format.ToSkColorType(),
                    SKAlphaType.Premul);

                using var properties = new SKSurfaceProperties(SKPixelGeometry.Unknown);

                // It is not too expensive to re-create the SKSurface on each re-paint.
                // See: https://groups.google.com/g/skia-discuss/c/3c10MvyaSug/m/UOr238asCgAJ
                //
                // When creating the SKSurface it is important to specify a pixel geometry
                // A defined pixel geometry is required for some anti-aliasing algorithms such as ClearType
                // Also see: https://github.com/AvaloniaUI/Avalonia/pull/9558
                using (var surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes, properties)) {
                    draw(surface.Canvas, token);
                }
            }
        }
    }
}
