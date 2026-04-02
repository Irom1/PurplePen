using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;

namespace AvUtil
{
    // An Avalonia control that can be drawn on using SkiaSharp drawing commands.
    // Handle the Paint event to draw to the canvas using SkiaSharp.
    // Use InvalidateSurface to trigger a repaint of the canvas which will fire the Paint event.
    //
    // Adapted from the Avalonia Labs SKCanvasView.
    // (https://github.com/AvaloniaUI/Avalonia.Labs/tree/main/src/Avalonia.Labs.Controls/SKCanvasView)
    public class SkiaDrawingView: Control
    {
        /// <summary>
        /// Event that is fired when the view should be painted.
        /// </summary>
        public event EventHandler<PaintEventArgs>? Paint;

        private WriteableBitmapTracker? _writeableBitmapTracker = null;
        private int _pixelWidth;
        private int _pixelHeight;
        private Size _logicalSize;
        private double _scale = 1;

        // This is set to write when posting a repaint event, and set back to false when
        // repainting begins. This is used to prevent multiple repaints from being queued.
        private volatile bool repaintQueued = false;

        /// <summary>
        /// Gets the current pixel size of the canvas.
        /// Any scaling factor is already applied.
        /// </summary>
        public Size CanvasSize => new Size(_pixelWidth, _pixelHeight);

        /// <summary>
        /// Invalidates the canvas causing the surface to be repainted.
        /// This will fire the <see cref="Paint"/> event.
        /// </summary>
        public void InvalidateSurface()
        {
            if (!repaintQueued) {
                repaintQueued = true;
                Dispatcher.UIThread.Post(RepaintSurface);
            }
        }

        /// <summary>
        /// Repaints the Skia surface and canvas.
        /// </summary>
        private void RepaintSurface()
        {
            repaintQueued = false;

            // Painting in design mode causes problems in the designer.

            if (!Design.IsDesignMode) {
                // WriteableBitmap does not support zero-size dimensions
                // Therefore, to avoid a crash, exit here if size is zero
                if (!this.IsVisible || _pixelWidth == 0 || _pixelHeight == 0) {
                    _writeableBitmapTracker?.Dispose();
                    _writeableBitmapTracker = null;
                    return;
                }

                if (_writeableBitmapTracker != null && (_writeableBitmapTracker.Bitmap.PixelSize.Width != _pixelWidth || _writeableBitmapTracker.Bitmap.PixelSize.Height != _pixelHeight)) {
                    _writeableBitmapTracker?.Dispose();
                    _writeableBitmapTracker = null;
                }

                _writeableBitmapTracker ??= WriteableBitmapPool.Instance.Rent(new PixelSize(_pixelWidth, _pixelHeight), longLived: true);

                SkiaWritableBitmap.DrawToBitmap(_writeableBitmapTracker,
                    (SKCanvas canvas, CancellationToken cancelToken) => {
                        canvas.Scale(Convert.ToSingle(_scale));
                        this.OnPaint(new PaintEventArgs(canvas, _logicalSize, new SKSizeI(_pixelWidth, _pixelHeight), _scale, cancelToken));
                    });

                InvalidateVisual();
            }

            return;
        }

        /// <summary>
        /// Called when the canvas should repaint its surface.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnPaint(PaintEventArgs e)
        {
            this.Paint?.Invoke(this, e);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _writeableBitmapTracker?.Dispose();
            _writeableBitmapTracker = null;
        }

        public sealed override void Render(DrawingContext context)
        {
            if (_writeableBitmapTracker != null) {
                Rect bounds = Bounds;
                int currentPixelWidth = Convert.ToInt32(bounds.Width * _scale);
                int currentPixelHeight = Convert.ToInt32(bounds.Height * _scale);

                context.DrawImage(_writeableBitmapTracker.Bitmap, new Rect(0, 0, currentPixelWidth, currentPixelHeight), new Rect(Bounds.Size));
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsVisibleProperty) {
                this.InvalidateSurface();
            }
            if (change.Property == BoundsProperty) {
                // Display scaling is important to consider here:
                // The bitmap itself should be sized to match physical device pixels.
                // This ensures it is never pixelated and renders properly to the display.
                // However, in several cases physical pixels do not match the logical pixels.
                // We also don't want to have to consider scaling in external code when calculating graphics.
                // To make this easiest, the layout scaling factor is calculated and then used
                // to find the size of the bitmap. This ensures it will match device pixels.
                // Then the canvas undoes this by setting a scale factor itself.
                // This means external code can use logical pixel size and the canvas will transform as needed.
                // Then the underlying bitmap is still at physical device pixel resolution.

                _scale = LayoutHelper.GetLayoutScale(this);
                var bounds = change.GetNewValue<Rect>();
                _logicalSize = bounds.Size;
                _pixelWidth = Convert.ToInt32(bounds.Width * _scale);
                _pixelHeight = Convert.ToInt32(bounds.Height * _scale);
                this.InvalidateSurface();
            }
            return;
        }

        public sealed class PaintEventArgs : EventArgs
        {
            public PaintEventArgs(SKCanvas canvas, Size logicalSize, SKSizeI pixelSize, double scale, CancellationToken cancelToken)
            {
                Canvas = canvas;
                LogicalSize = logicalSize;
                PixelSize = pixelSize;
                Scale = scale;
                CancellationToken = cancelToken;
            }

            public SKCanvas Canvas { get; }
            public Size LogicalSize { get; }

            public SKSizeI PixelSize { get; }

            public double Scale { get;  }

            public CancellationToken CancellationToken { get; }
        }
    }
}

