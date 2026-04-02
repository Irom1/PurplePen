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
    // A thread-safe pool that reuses WriteableBitmap instances to avoid repeated
    // large object heap allocations and reduce GC pressure.
    public class WriteableBitmapPool : IDisposable
    {
        public static WriteableBitmapPool Instance { get; } = new WriteableBitmapPool();

        // Maximum number of bitmaps kept in the pool.
        public const int MaxPoolSize = 6;

        private WriteableBitmapPool() { }

        private readonly List<WriteableBitmapTracker> pool = new List<WriteableBitmapTracker>();
        private readonly object lockObj = new object();
        private bool isDisposed;

        // Rent a bitmap at least as large as requestedSize. Returns the smallest
        // pooled bitmap that encloses the requested dimensions, or creates a new one.
        // The bitmap is cleared to transparent before being returned.
        // If longLived is true, a pooled bitmap will not be rented if it has more than
        // 2x the requested pixel count, to avoid tying up an oversized bitmap for a long time.
        public WriteableBitmapTracker Rent(PixelSize requestedSize, bool longLived = false)
        {
            WriteableBitmapTracker? tracker = null;
            long requestedPixelCount = (long)requestedSize.Width * requestedSize.Height;

            lock (lockObj) {
                if (isDisposed)
                    throw new ObjectDisposedException(nameof(WriteableBitmapPool));

                // Find the smallest bitmap that encloses the requested size.
                int bestIndex = -1;
                long bestPixelCount = long.MaxValue;

                for (int i = 0; i < pool.Count; i++) {
                    PixelSize bitmapSize = pool[i].Bitmap.PixelSize;
                    if (bitmapSize.Width >= requestedSize.Width && bitmapSize.Height >= requestedSize.Height) {
                        long pixelCount = (long)bitmapSize.Width * bitmapSize.Height;

                        // For long-lived rentals, skip bitmaps that are more than 2x the requested size.
                        if (longLived && pixelCount > requestedPixelCount * 2)
                            continue;

                        if (pixelCount < bestPixelCount) {
                            bestPixelCount = pixelCount;
                            bestIndex = i;
                        }
                    }
                }

                if (bestIndex >= 0) {
                    tracker = pool[bestIndex];
                    pool.RemoveAt(bestIndex);
                }
            }

            // Create a new bitmap if none in the pool was large enough.
            if (tracker == null) {
                WriteableBitmap bitmap = new WriteableBitmap(
                    requestedSize,
                    new Vector(96, 96),
                    SKImageInfo.PlatformColorType.ToPixelFormat(),
                    AlphaFormat.Premul);
                tracker = new WriteableBitmapTracker(bitmap);
            }

            tracker.SetRentalSize(requestedSize);
            ClearToTransparent(tracker);

            return tracker;
        }

        // Return a bitmap to the pool for reuse. If the pool is already at MaxPoolSize,
        // the smallest bitmap in the pool is disposed to make room.
        public void Return(WriteableBitmapTracker tracker)
        {
            lock (lockObj) {
                if (isDisposed) {
                    tracker.Dispose();
                    return;
                }

                pool.Add(tracker);

                if (pool.Count > MaxPoolSize) {
                    // Find and dispose the smallest bitmap by pixel count.
                    int smallestIndex = 0;
                    long smallestPixelCount = long.MaxValue;

                    for (int i = 0; i < pool.Count; i++) {
                        long pixelCount = (long)pool[i].Bitmap.PixelSize.Width * pool[i].Bitmap.PixelSize.Height;
                        if (pixelCount < smallestPixelCount) {
                            smallestPixelCount = pixelCount;
                            smallestIndex = i;
                        }
                    }

                    pool[smallestIndex].Dispose();
                    pool.RemoveAt(smallestIndex);
                }
            }
        }

        // Dispose all bitmaps remaining in the pool.
        public void Dispose()
        {
            lock (lockObj) {
                isDisposed = true;

                foreach (WriteableBitmapTracker tracker in pool) {
                    tracker.Dispose();
                }

                pool.Clear();
            }
        }

        // Clear the entire bitmap to transparent using Skia.
        private static void ClearToTransparent(WriteableBitmapTracker tracker)
        {
            using (ILockedFramebuffer framebuffer = tracker.Bitmap.Lock()) {
                SKImageInfo info = new SKImageInfo(
                    framebuffer.Size.Width,
                    framebuffer.Size.Height,
                    framebuffer.Format.ToSkColorType(),
                    SKAlphaType.Premul);

                using SKSurfaceProperties properties = new SKSurfaceProperties(SKPixelGeometry.Unknown);
                using SKSurface surface = SKSurface.Create(info, framebuffer.Address, framebuffer.RowBytes, properties);
                surface.Canvas.Clear(SKColors.Transparent);
            }
        }
    }
}
