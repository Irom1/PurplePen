using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AvUtil
{
    // Pans and zooms a drawing. This displays an IAvaloniaDrawing that applies a pan and zoom transform to it.
    public class PanAndZoom : Control, ICustomHitTest
    {
        IAvaloniaDrawing? cachedDrawing;

        // Defines what part of the map we are viewing.
        private Point centerPoint = new Point(0, 0);			// center point in world coordinates.
        private float zoom = 1.0F;							    // zoom, 1.0 == approx real world size.
        private readonly float pixelPerMm;						// number of pixel/mm on this display
        private Matrix xformWorldToPixel;						// transformation world->pixel coord
        private Matrix xformPixelToWorld;						// transformation pixel->world coord
        private Rect viewport;							        // visible area in world coordinates

        private float minZoom = 0.1F, maxZoom = 100F;           // limits of zoom.

        bool dragScrollingInProgress = false;					// Are we dragging the map around?
        MouseButton endDragScrollButton;                        // Which mouse button ends drag scrolling.
        Point lastDragScrollPoint;								// last point we dragged to

        public PanAndZoom()
        {
            pixelPerMm = 96 / 25.4F;  // 96 pixels is the standard DPI, which is what is used everywhere in Avalonia.
            xformPixelToWorld = new Matrix();
            xformWorldToPixel = new Matrix();


            this.IsHitTestVisible = true;
        }

        public IAvaloniaDrawing? Drawing {
            get { return cachedDrawing; }
            set {
                if (cachedDrawing != value) {
                    if (cachedDrawing != null)
                        cachedDrawing.DrawingChanged -= CachedDrawing_NewDrawingAvailable;

                    cachedDrawing = value;

                    if (cachedDrawing != null)
                        cachedDrawing.DrawingChanged += CachedDrawing_NewDrawingAvailable;

                    ViewportChanged();
                }
            }
        }

        public Point CenterPoint {
            get { return centerPoint; }
            set {
                if (centerPoint != value) {
                    //centerPoint = ConstrainCenterPoint(value, viewport.Size, GetScrollBounds());
                    centerPoint = value;
                    ViewportChanged();
                }
            }
        }

        public float ZoomFactor {
            get { return zoom; }
            set {
                // clamp zoom to a reasonable value.
                if (value < minZoom)
                    value = minZoom;
                if (value > maxZoom)
                    value = maxZoom;

                if (zoom != value) {
                    zoom = value;
                    ViewportChanged();
                }
            }
        }

        static int renderNumber = 0;

        public override void Render(DrawingContext context)
        {
            Rect bounds = new Rect(Bounds.Size);  // Bounds in my coordinates, starting at 0,0

            if (cachedDrawing != null) {
                ++renderNumber;
                Debug.WriteLine($"Beginning Render {renderNumber}");

                Stopwatch watch = new Stopwatch();
                watch.Start();

                double scale = LayoutHelper.GetLayoutScale(this);
                int pixelWidth = (int)Math.Ceiling(bounds.Width * scale);
                int pixelHeight = (int)Math.Ceiling(bounds.Height * scale);
                PixelSize pixelSize = new PixelSize(pixelWidth, pixelHeight);

                context.PushTransform(xformWorldToPixel);
                cachedDrawing.Draw(context, viewport, pixelSize);

                watch.Stop();

                Debug.WriteLine($"Ending Render {renderNumber} {watch.ElapsedMilliseconds}ms");

            }
            else {
                context.FillRectangle(Brushes.Aqua, bounds);
            }
        }

        private void CachedDrawing_NewDrawingAvailable(object? sender, EventArgs e)
        {
            Debug.WriteLine("New Drawing Available");
            InvalidateVisual();
        }


        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            ViewportChanged();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            PointerPoint pointer = e.GetCurrentPoint(this);
            PointerPointProperties props = pointer.Properties;

            Debug.WriteLine("Pointer Pressed " + props.PointerUpdateKind);

            if (props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                BeginMapDragging(pointer.Position, MouseButton.RightButton);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            PointerPoint pointer = e.GetCurrentPoint(this);
            PointerPointProperties props = pointer.Properties;

            Debug.WriteLine("Pointer Released " + props.PointerUpdateKind);

            if (props.PointerUpdateKind == PointerUpdateKind.RightButtonReleased && endDragScrollButton == MouseButton.RightButton) {
                EndMapDragging(pointer.Position);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            PointerPoint pointer = e.GetCurrentPoint(this);
            PointerPointProperties props = pointer.Properties;

            if (dragScrollingInProgress)
                MapDrag(pointer.Position);
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            const double zoomChange = 1.1892;  // The four root of 2. So four scrolls zooms by 2x.

            base.OnPointerWheelChanged(e);

            // Retrieve the delta of the wheel. 1 is a single wheel notch (not 120 like WinForms)
            double delta = e.Delta.Y;

            // Determine the point to zoom around.
            Avalonia.Rect rect = new Rect(this.Bounds.Size);  // local coordinates.
            Point zoomPtPixel, zoomPtWorld;

            Point pt = e.GetPosition(this);
            if (rect.Contains(pt))
                zoomPtPixel = pt;
            else
                return;

            zoomPtWorld = PixelToWorld(zoomPtPixel);

            // Determine the new zoom factor.
            double zoomAmount = Math.Pow(zoomChange, Math.Abs(delta));
            float newZoom;
            if (delta > 0) {
                newZoom = ZoomFactor * (float)zoomAmount;
            }
            else {
                newZoom = ZoomFactor / (float)zoomAmount;
            }

            ZoomAroundPoint(zoomPtWorld, newZoom);


            // Mark the event as handled if needed
            e.Handled = true;
        }

        float ScaleFactor {
            get { return pixelPerMm * zoom; }
        }

        void CalculateWorldTransform()
        {
            // Get size, midpoint of the window .
            Size sizeInPixels = this.Bounds.Size;  
            Point midpoint = new Point(sizeInPixels.Width / 2.0F, sizeInPixels.Height / 2.0F);

            // Calculate the world->window transform.
            float scaleFactor = ScaleFactor;
            xformWorldToPixel = Matrix.CreateTranslation(-centerPoint.X, -centerPoint.Y) * 
                                Matrix.CreateScale(scaleFactor, -scaleFactor) * 
                                Matrix.CreateTranslation(midpoint.X, midpoint.Y);

            // Invert it to get the window->world transform.
            xformPixelToWorld = xformWorldToPixel.Invert();

            // Calculate the viewport in world coordinates.
            Point[] pts = { new Point(0.0F, (float)sizeInPixels.Height), new Point((float)sizeInPixels.Width, 0.0F) };
            Point pt0 = xformPixelToWorld.Transform(new Point(0.0, sizeInPixels.Height));
            Point pt1 = xformPixelToWorld.Transform(new Point(sizeInPixels.Width, 0.0));
            viewport = new Rect(pt0.X, pt0.Y, pt1.X - pt0.X, pt1.Y - pt0.Y);
        }

        // Transform rectangle from world to pixel coordinates. 
        public Rect WorldToPixel(Rect rectWorld)
        {
            Point pt0 = xformWorldToPixel.Transform(new Point(rectWorld.Left, rectWorld.Top));
            Point pt1 = xformWorldToPixel.Transform(new Point(rectWorld.Right, rectWorld.Bottom));
            
            // Note that Y's are reversed, so we reverse the rectangle to make the rect height always positive.
            return new Rect(new Point(pt0.X, pt1.Y), new Size(pt1.X - pt0.X, pt0.Y - pt1.Y));
        }

        // Transform rectangle from pixel to world coordinates. 
        public Rect PixelToWorld(Rect rectPixel)
        {
            Point pt0 = xformPixelToWorld.Transform(new Point(rectPixel.Left, rectPixel.Top));
            Point pt1 = xformPixelToWorld.Transform(new Point(rectPixel.Right, rectPixel.Bottom));

            // Note that Y's are reversed, so we reverse the rectangle to make the rect height always positive.
            return new Rect(new Point(pt0.X, pt1.Y), new Size(pt1.X - pt0.X, pt0.Y - pt1.Y));
        }

        // Transform one point from world to pixel coordinates. 
        public Point WorldToPixel(Point ptWorld)
        {
            return xformWorldToPixel.Transform(ptWorld);
        }

        // Transform one point from pixel to world coordinates. 
        public Point PixelToWorld(Point ptPixel)
        {
            return xformPixelToWorld.Transform(ptPixel);
        }

        // Transform distance from world to pixel coordinates. 
        public float WorldToPixelDistance(float distWorld)
        {
            // M11 is the X scale, which is the same as Y scale since we use uniform scaling.
            return (float) (distWorld * xformWorldToPixel.M11);  
        }

        // Transform distance from pixe to world coordinates. 
        public float PixelToWorldDistance(float distPixel)
        {
            // M11 is the X scale, which is the same as Y scale since we use uniform scaling.
            return (float)(distPixel * xformPixelToWorld.M11);
        }

        // Zoom, keeping the given point at the same location in pixel coordinates.
        public void ZoomAroundPoint(Point zoomPtWorld, float newZoom)
        {
            Point zoomPtPixel = WorldToPixel(zoomPtWorld);
            ZoomFactor = newZoom;
            Point zoomPtWorldNew = PixelToWorld(zoomPtPixel);
            Point centerPtWorld = new Point(CenterPoint.X + (zoomPtWorld.X - zoomPtWorldNew.X), CenterPoint.Y + (zoomPtWorld.Y - zoomPtWorldNew.Y));
            CenterPoint = centerPtWorld;
        }

        void BeginMapDragging(Point pt, MouseButton endingButton)
        {
            dragScrollingInProgress = true;
            endDragScrollButton = endingButton;
            lastDragScrollPoint = pt;
            //this.Cursor = DragCursor;
            //DisableHoverTimer();
        }

        void EndMapDragging(Point pt)
        {
            dragScrollingInProgress = false;
            //this.Cursor = Cursors.Default;
        }

        void MapDrag(Point pt)
        {
            Debug.Assert(dragScrollingInProgress);

            Point worldLastDrag = PixelToWorld(lastDragScrollPoint);
            Point worldCurrentDrag = PixelToWorld(pt);

            CenterPoint = new Point(centerPoint.X + worldLastDrag.X - worldCurrentDrag.X, centerPoint.Y + worldLastDrag.Y - worldCurrentDrag.Y);

            lastDragScrollPoint = pt;
        }

        void ViewportChanged()
        {
            CalculateWorldTransform();
            this.InvalidateVisual();
        }

        // Always be hittable, even if we don't draw anything.
        public bool HitTest(Avalonia.Point point)
        {
            // You have to check bounds, or else you get hit testing outside the control bounds.
            Rect controlBounds = new Rect(Bounds.Size);
            return controlBounds.Contains(point);
        }

        enum MouseButton { LeftButton, RightButton, MiddleButton }
    }
}
