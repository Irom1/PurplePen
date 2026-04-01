using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AvUtil
{
    // A class that tracks a WriteableBitmap and ensures it is disposed properly.
    // If the bitmap is not disposed, it logs a warning with the stack trace of where it was allocated.
    public class WriteableBitmapTracker : IDisposable
    {
        public WriteableBitmap Bitmap { get; }
        private bool _isDisposed;
        private readonly string _allocationStackTrace;

        public WriteableBitmapTracker(WriteableBitmap bitmap)
        {
            Bitmap = bitmap;
            // Capture where this bitmap was created to make debugging easy
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

                // Optional: Clean up the bitmap here as a failsafe, though doing 
                // unmanaged cleanup inside a finalizer thread can sometimes be risky depending on the graphics context.
            }
        }
    }
}
