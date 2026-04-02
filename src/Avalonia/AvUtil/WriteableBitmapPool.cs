using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Timers;

namespace AvUtil
{
    // A thread-safe pool that reuses WriteableBitmap instances to avoid repeated
    // large object heap allocations and reduce GC pressure.
    //
    // I'm not sure is this was really that important. It seemed like a good idea at the time,
    // but I haven't been able to conclusively demonstrate that it improves performance or reduces GC.
    public class WriteableBitmapPool : IDisposable
    {
        public static WriteableBitmapPool Instance { get; } = new WriteableBitmapPool();

        // Maximum number of bitmaps kept in the pool.
        public const int MaxPoolSize = 6;

        // How often to trim the pool for idle bitmaps, in seconds.
        private const double TrimIntervalSeconds = 10;

        private WriteableBitmapPool()
        {
            trimTimer = new System.Timers.Timer(TrimIntervalSeconds * 1000);
            trimTimer.Elapsed += (s, e) => OnTimerTick();
            trimTimer.AutoReset = true;
            trimTimer.Start();
        }

        private readonly List<PoolEntry> pool = new List<PoolEntry>();
        private readonly object lockObj = new object();
        private readonly System.Timers.Timer trimTimer;
        private bool isDisposed;

        // Total number of bytes used by all bitmaps currently in the pool.
        // Assumes 4 bytes per pixel (BGRA/RGBA premultiplied).
        public long TotalBytes
        {
            get {
                lock (lockObj) {
                    long total = 0;
                    for (int i = 0; i < pool.Count; i++) {
                        PixelSize size = pool[i].Tracker.Bitmap.PixelSize;
                        total += (long)size.Width * size.Height * 4;
                    }
                    return total;
                }
            }
        }

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
                    PixelSize bitmapSize = pool[i].Tracker.Bitmap.PixelSize;

                    // I used to check for sizes > the requested size, but I was getting weird bugs. Trying just
                    // equal sizes.
                    if (bitmapSize.Width == requestedSize.Width && bitmapSize.Height == requestedSize.Height) {
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
                    tracker = pool[bestIndex].Tracker;
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

                pool.Add(new PoolEntry(tracker));

                if (pool.Count > MaxPoolSize) {
                    // Find and dispose the smallest bitmap by pixel count.
                    int smallestIndex = 0;
                    long smallestPixelCount = long.MaxValue;

                    for (int i = 0; i < pool.Count; i++) {
                        long pixelCount = (long)pool[i].Tracker.Bitmap.PixelSize.Width * pool[i].Tracker.Bitmap.PixelSize.Height;
                        if (pixelCount < smallestPixelCount) {
                            smallestPixelCount = pixelCount;
                            smallestIndex = i;
                        }
                    }

                    pool[smallestIndex].Tracker.Dispose();
                    pool.RemoveAt(smallestIndex);
                }

            }
        }

        // Trim bitmaps that have been idle longer than the specified duration.
        // Returns the number of bitmaps trimmed.
        public int TrimOlderThan(TimeSpan maxIdle)
        {
            long now = Environment.TickCount64;
            long maxIdleMs = (long)maxIdle.TotalMilliseconds;
            int trimmed = 0;

            lock (lockObj) {
                for (int i = pool.Count - 1; i >= 0; i--) {
                    if (now - pool[i].ReturnedTickCount >= maxIdleMs) {
                        pool[i].Tracker.Dispose();
                        pool.RemoveAt(i);
                        trimmed++;
                    }
                }
            }

            return trimmed;
        }

        // Dispose all bitmaps remaining in the pool and stop the trim timer.
        public void Dispose()
        {
            trimTimer.Dispose();

            lock (lockObj) {
                isDisposed = true;

                foreach (PoolEntry entry in pool) {
                    entry.Tracker.Dispose();
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

        // Called periodically by the timer on a thread pool thread.
        // Trims bitmaps idle longer than the trim interval.
        // Wrapped in try-catch to ensure the timer keeps firing even if an exception occurs.
        private void OnTimerTick()
        {
            int trimmed = TrimOlderThan(TimeSpan.FromSeconds(TrimIntervalSeconds));
        }

        // Tracks a pooled bitmap along with the time it was returned to the pool.
        private readonly struct PoolEntry
        {
            public WriteableBitmapTracker Tracker { get; }
            public long ReturnedTickCount { get; }

            public PoolEntry(WriteableBitmapTracker tracker)
            {
                Tracker = tracker;
                ReturnedTickCount = Environment.TickCount64;
            }
        }
    }

}
