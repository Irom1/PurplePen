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
        public static WriteableBitmap DrawToBitmap(PixelSize pixelSize, Action<SKCanvas> draw)
        {
            return DrawToBitmap(pixelSize, (canvas, _) => draw(canvas));
        }

        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static WriteableBitmap DrawToBitmap(PixelSize pixelSize, Action<SKCanvas, CancellationToken> draw, CancellationToken token = default)
        {
            WriteableBitmap bitmap = new WriteableBitmap(
                pixelSize,
                new Vector(96, 96),
                SKImageInfo.PlatformColorType.ToPixelFormat(),
                AlphaFormat.Premul);

            DrawToBitmap(bitmap, draw, token);
            return bitmap;
        }

        // Create a new bitmap of the given size, and draw to it using the given drawing function.
        public static async Task<WriteableBitmap> DrawToBitmapAsync(PixelSize pixelSize, Func<SKCanvas, CancellationToken, Task> draw, CancellationToken token = default)
        {
            SKColorType colorType = SKImageInfo.PlatformColorType;

            WriteableBitmap bitmap = new WriteableBitmap(
                pixelSize,
                new Vector(96, 96),
                colorType.ToPixelFormat(),
                AlphaFormat.Premul);

            using (var framebuffer = bitmap.Lock()) {
                var info = new SKImageInfo(
                    framebuffer.Size.Width,
                    framebuffer.Size.Height,
                    colorType,
                    SKAlphaType.Premul);

                using var properties = new SKSurfaceProperties(SKPixelGeometry.Unknown);

                using (var surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes, properties)) {
                    await draw(surface.Canvas, token);
                }
            }

            return bitmap;
        }

        // Draw to an existing WriteableBitmap using the given drawing function.
        public static void DrawToBitmap(WriteableBitmap bitmap, Action<SKCanvas, CancellationToken> draw, CancellationToken token = default)
        {
            using (var framebuffer = bitmap.Lock()) {
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
