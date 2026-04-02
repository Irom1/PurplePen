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
    public static class SkiaWritableBitmap
    {
        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static WriteableBitmapTracker DrawToBitmap(PixelSize pixelSize, Action<SKCanvas> draw)
        {
            return DrawToBitmap(pixelSize, (canvas, _) => draw(canvas));
        }

        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static WriteableBitmapTracker DrawToBitmap(PixelSize pixelSize, Action<SKCanvas, CancellationToken> draw, bool longLived = false, CancellationToken token = default)
        {
            WriteableBitmapTracker bitmapTracker = WriteableBitmapPool.Instance.Rent(pixelSize, longLived);

            DrawToBitmap(bitmapTracker, draw, token);

            return bitmapTracker;
        }

        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static async Task<WriteableBitmapTracker> DrawToBitmapAsync(PixelSize pixelSize, Func<SKCanvas, CancellationToken, Task> draw, bool longLived = false, CancellationToken token = default)
        {
            SKColorType colorType = SKImageInfo.PlatformColorType;

            WriteableBitmapTracker bitmapTracker = WriteableBitmapPool.Instance.Rent(pixelSize, longLived);

            try {
                using (ILockedFramebuffer framebuffer = bitmapTracker.Bitmap.Lock()) {
                    SKImageInfo info = new SKImageInfo(
                        framebuffer.Size.Width,
                        framebuffer.Size.Height,
                        colorType,
                        SKAlphaType.Premul);

                    using (SKSurfaceProperties properties = new SKSurfaceProperties(SKPixelGeometry.Unknown)) 
                    using (SKSurface surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes, properties)) {
                        await draw(surface.Canvas, token);
                    }
                }
            }
            catch {
                // Clean up bitmap that we aren't going to return if the drawing was cancelled.
                bitmapTracker.Dispose();
                throw;
            }

            return bitmapTracker;
        }

        // Draw to an existing WriteableBitmap using the given drawing function.
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
