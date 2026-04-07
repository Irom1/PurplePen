using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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
    // Use InvalidateRect to invalidate a rectangle; a repaint will only fire if that rectangle is in view.
    //
    // It implements the ILogicalScrollable interface, so it can be used with ScrollViewer to provide
    // scrolling functionality when the content is larger than the view.
    //
    // Adapted from the Avalonia Labs SKCanvasView.
    // (https://github.com/AvaloniaUI/Avalonia.Labs/tree/main/src/Avalonia.Labs.Controls/SKCanvasView)
    public class SkiaScrollableDrawingView: Control, ILogicalScrollable
    {
        /// <summary>
        /// Defines the <see cref="LogicalExtent"/> property.
        /// </summary>
        public static readonly StyledProperty<Size> LogicalExtentProperty =
            AvaloniaProperty.Register<SkiaScrollableDrawingView, Size>(
                nameof(LogicalExtent),
                defaultValue: default,
                coerce: CoerceLogicalExtent);

        /// <summary>
        /// Defines the <see cref="ScrollChangeFraction"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ScrollChangeFractionProperty =
            AvaloniaProperty.Register<SkiaScrollableDrawingView, double>(
                nameof(ScrollChangeFraction),
                defaultValue: 0.05,
                coerce: CoerceScrollChangeFraction);

        /// <summary>
        /// Event that is fired when the view should be painted.
        /// </summary>
        public event EventHandler<PaintEventArgs>? Paint;

        private WriteableBitmapTracker? _writeableBitmapTracker = null;
        private int _pixelWidth;
        private int _pixelHeight;
        private Size _logicalSize;
        private Vector _offset;
        private bool _canHorizontallyScroll = true;
        private bool _canVerticallyScroll = true;
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
        /// Gets or sets the logical extent (total scrollable content size) of the drawing surface.
        /// </summary>
        public Size LogicalExtent {
            get => GetValue(LogicalExtentProperty);
            set => SetValue(LogicalExtentProperty, value);
        }

        /// <summary>
        /// Gets or sets the fraction of the viewport used for logical scroll changes.
        /// </summary>
        public double ScrollChangeFraction {
            get => GetValue(ScrollChangeFractionProperty);
            set => SetValue(ScrollChangeFractionProperty, value);
        }

        private static Size CoerceLogicalExtent(AvaloniaObject obj, Size value)
        {
            return new Size(Math.Max(0, value.Width), Math.Max(0, value.Height));
        }

        private static double CoerceScrollChangeFraction(AvaloniaObject obj, double value)
        {
            return Math.Max(0, value);
        }

        public bool CanHorizontallyScroll {
            get => _canHorizontallyScroll;
            set {
                if (_canHorizontallyScroll != value) {
                    _canHorizontallyScroll = value;
                    Offset = CoerceOffset(_offset);
                    RaiseScrollInvalidated(EventArgs.Empty);
                }
            }
        }

        public bool CanVerticallyScroll {
            get => _canVerticallyScroll;
            set {
                if (_canVerticallyScroll != value) {
                    _canVerticallyScroll = value;
                    Offset = CoerceOffset(_offset);
                    RaiseScrollInvalidated(EventArgs.Empty);
                }
            }
        }

        public bool IsLogicalScrollEnabled => true;

        public Size ScrollSize => new Size(
            CanHorizontallyScroll ? _logicalSize.Width * ScrollChangeFraction : 0,
            CanVerticallyScroll ? _logicalSize.Height * ScrollChangeFraction : 0);

        public Size PageScrollSize => _logicalSize;

        public Size Extent => LogicalExtent;

        public Vector Offset {
            get => _offset;
            set {
                var coercedOffset = CoerceOffset(value);
                if (_offset != coercedOffset) {
                    _offset = coercedOffset;
                    RaiseScrollInvalidated(EventArgs.Empty);
                    InvalidateSurface();
                }
            }
        }

        Size IScrollable.Viewport => _logicalSize;

        // The part of the context that is currently visible.
        public Rect VisibleRect => new Rect(new Point(_offset.X, _offset.Y), _logicalSize);


        public event EventHandler? ScrollInvalidated;

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
        /// Invalidates the canvas causing the surface to be repainted.
        /// This will fire the <see cref="Paint"/> event.
        /// </summary>
        public void InvalidateRect(Rect rectInvalid)
        {
            // If the invalid rectangle is not visible, there is nothing to do; 
            // it will get repainted when it scrolls into view.
            if (!rectInvalid.Intersects(VisibleRect))
                return;

            if (!repaintQueued) {
                repaintQueued = true;
                Dispatcher.UIThread.Post(RepaintSurface);
            }
        }

        public PointerPoint GetPointerPoint(PointerEventArgs pointerEventArgs)
        {
            PointerPoint pointerPoint = pointerEventArgs.GetCurrentPoint(this);
            Point position = pointerPoint.Position;
            Point scrolledPosition = new Point(position.X + _offset.X, position.Y + _offset.Y);
            return new PointerPoint(pointerPoint.Pointer, scrolledPosition, pointerPoint.Properties);
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
                    _writeableBitmapTracker.Dispose();
                    _writeableBitmapTracker = null;
                }

                _writeableBitmapTracker ??= new WriteableBitmapTracker(new PixelSize(_pixelWidth, _pixelHeight));

                SkiaWriteableBitmapUtil.DrawToBitmap(_writeableBitmapTracker,
                    (SKCanvas canvas, CancellationToken cancelToken) => {
                        canvas.SetMatrix(SKMatrix.CreateScaleTranslation(Convert.ToSingle(_scale), Convert.ToSingle(_scale), Convert.ToSingle(-_offset.X * _scale), Convert.ToSingle(-_offset.Y * _scale)));
                        this.OnPaint(new PaintEventArgs(canvas, new Rect(new Point(_offset.X, _offset.Y), _logicalSize), new SKSizeI(_pixelWidth, _pixelHeight), _scale, cancelToken));
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

        public bool BringIntoView(Control target, Rect targetRect)
        {
            if (target != this) {
                return false;
            }

            var newOffset = _offset;

            if (CanHorizontallyScroll) {
                if (targetRect.Left < newOffset.X) {
                    newOffset = new Vector(targetRect.Left, newOffset.Y);
                }
                else if (targetRect.Right > newOffset.X + _logicalSize.Width) {
                    newOffset = new Vector(targetRect.Right - _logicalSize.Width, newOffset.Y);
                }
            }

            if (CanVerticallyScroll) {
                if (targetRect.Top < newOffset.Y) {
                    newOffset = new Vector(newOffset.X, targetRect.Top);
                }
                else if (targetRect.Bottom > newOffset.Y + _logicalSize.Height) {
                    newOffset = new Vector(newOffset.X, targetRect.Bottom - _logicalSize.Height);
                }
            }

            Offset = newOffset;
            return true;
        }

        // Scroll the target rectangle into view.
        public void BringIntoView(Rect targetRect)
        {
            BringIntoView(this, targetRect);
        }

        public Control? GetControlInDirection(NavigationDirection direction, Control? from)
        {
            return null;
        }

        public void RaiseScrollInvalidated(EventArgs e)
        {
            ScrollInvalidated?.Invoke(this, e);
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

                context.PushClip(new Rect(Bounds.Size));
                context.DrawImage(_writeableBitmapTracker.Bitmap, new Rect(0, 0, currentPixelWidth, currentPixelHeight), new Rect(Bounds.Size));
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsVisibleProperty) {
                this.InvalidateSurface();
            }
            if (change.Property == LogicalExtentProperty) {
                Offset = CoerceOffset(_offset);
                RaiseScrollInvalidated(EventArgs.Empty);
                InvalidateSurface();
            }
            if (change.Property == ScrollChangeFractionProperty) {
                RaiseScrollInvalidated(EventArgs.Empty);
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
                Offset = CoerceOffset(_offset);
                RaiseScrollInvalidated(EventArgs.Empty);
                this.InvalidateSurface();
            }
            return;
        }

        private Vector CoerceOffset(Vector offset)
        {
            double maxX = CanHorizontallyScroll ? Math.Max(0, Extent.Width - _logicalSize.Width) : 0;
            double maxY = CanVerticallyScroll ? Math.Max(0, Extent.Height - _logicalSize.Height) : 0;

            return new Vector(
                Math.Clamp(offset.X, 0, maxX),
                Math.Clamp(offset.Y, 0, maxY));
        }

        public sealed class PaintEventArgs : EventArgs
        {
            public PaintEventArgs(SKCanvas canvas, Rect logicalViewPort, SKSizeI pixelSize, double scale, CancellationToken cancelToken)
            {
                Canvas = canvas;
                LogicalViewPort = logicalViewPort;
                PixelSize = pixelSize;
                Scale = scale;
                CancellationToken = cancelToken;
            }

            public SKCanvas Canvas { get; }
            public Rect LogicalViewPort { get; }

            public SKSizeI PixelSize { get; }

            public double Scale { get;  }

            public CancellationToken CancellationToken { get; }
        }
    }
}

