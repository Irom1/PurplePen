using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        IAvaloniaDrawing? drawing;

        // Defines what part of the map we are viewing.
        private Point centerPoint = new Point(0, 0);			// center point in world coordinates.
        private float zoom = 1.0F;							    // zoom, 1.0 == approx real world size.
        private Rect viewport;							        // visible area in world coordinates
        private readonly float pixelPerMm;						// number of pixel/mm on this display
        private Matrix xformWorldToLogPixel;					// transformation world->logical pixel coord
        private Matrix xformLogPixelToWorld;				    // transformation logical pixel->world coord
        private Matrix xformWorldToPhysPixel;					// transformation world->physical pixel coord
        private Matrix xformPhysPixelToWorld;				    // transformation physical pixel->world coord

        private float minZoom = 0.1F, maxZoom = 100F;           // limits of zoom.

        bool panningInProgress = false;					        // Are we panning the map around by holding a button down?
        MouseButton endPanningButton;                           // Which mouse button ends panning.
        Point lastPanScrollPoint;								// last point we panned to, in logical pixels

        public static readonly RoutedEvent<MouseEventArgs> MouseActivityEvent =
            RoutedEvent.Register<PanAndZoom, MouseEventArgs>(
                name: nameof(MouseActivity),
                routingStrategy: RoutingStrategies.Direct);

        public PanAndZoom()
        {
            pixelPerMm = 96 / 25.4F;  // 96 pixels is the standard DPI, which is what is used everywhere in Avalonia.
            xformLogPixelToWorld = new Matrix();
            xformWorldToLogPixel = new Matrix();


            this.IsHitTestVisible = true;
        }

        // The drawing that we are drawing and panning/zooming over.
        public IAvaloniaDrawing? Drawing {
            get { return drawing; }
            set {
                if (drawing != value) {
                    if (drawing != null)
                        drawing.DrawingChanged -= Drawing_NewDrawingAvailable;

                    drawing = value;

                    if (drawing != null)
                        drawing.DrawingChanged += Drawing_NewDrawingAvailable;

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

        public event EventHandler<MouseEventArgs> MouseActivity {
            add => AddHandler(MouseActivityEvent, value);
            remove => RemoveHandler(MouseActivityEvent, value);
        }

        int renderNumber = 0;

        public override void Render(DrawingContext context)
        {
            Rect bounds = new Rect(Bounds.Size);  // Bounds in my coordinates, starting at 0,0

            if (drawing != null) {
                ++renderNumber;
                //Debug.WriteLine($"Beginning Pan/Zoom Render {renderNumber}");

                Stopwatch watch = new Stopwatch();
                watch.Start();

                double scale = LayoutHelper.GetLayoutScale(this);
                int pixelWidth = (int)Math.Ceiling(bounds.Width * scale);
                int pixelHeight = (int)Math.Ceiling(bounds.Height * scale);
                PixelSize pixelSize = new PixelSize(pixelWidth, pixelHeight);

                context.PushTransform(xformWorldToLogPixel);
                drawing.Draw(context, viewport, pixelSize, xformWorldToPhysPixel);

                watch.Stop();

                //Debug.WriteLine($"Ending Pan/Zoom Render {renderNumber} {watch.ElapsedMilliseconds}ms");

            }
            else {
                context.FillRectangle(Brushes.White, bounds);
            }
        }

        private void Drawing_NewDrawingAvailable(object? sender, EventArgs e)
        {
            //Debug.WriteLine("New Drawing Available");
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
            MouseButton mouseButton = MouseButtonFromPointerUpdate(props.PointerUpdateKind);

            if (mouseButton == MouseButton.Other)
                return;  // Ignore mouse buttons other than Left, Right, Middle.

            Point worldPos = PixelToWorld(pointer.Position);

            Debug.WriteLine("Pointer Pressed " + props.PointerUpdateKind + $" logpixel({pointer.Position.X},{pointer.Position.Y}) world({worldPos.X},{worldPos.Y})");

            MouseEventArgs eventArgs = new MouseEventArgs(MouseActivityEvent, this, mouseButton, PanAndZoom.MouseAction.Down, pointer.Position, worldPos);
            RaiseEvent(eventArgs);
            MouseDownResult result = eventArgs.MouseDownResult;

            if (result == MouseDownResult.BeginPanning) {
                BeginPanning(pointer.Position, mouseButton);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            PointerPoint pointer = e.GetCurrentPoint(this);
            PointerPointProperties props = pointer.Properties;
            MouseButton mouseButton = MouseButtonFromPointerUpdate(props.PointerUpdateKind);

            if (mouseButton == MouseButton.Other)
                return;  // Ignore mouse buttons other than Left, Right, Middle.

            Point worldPos = PixelToWorld(pointer.Position);

            Debug.WriteLine("Pointer Released " + props.PointerUpdateKind + $" logpixel({pointer.Position.X},{pointer.Position.Y}) world({worldPos.X},{worldPos.Y})");

            if (panningInProgress && mouseButton == endPanningButton) {
                EndPanning(pointer.Position);
            }
            else {
                MouseEventArgs eventArgs = new MouseEventArgs(MouseActivityEvent, this, mouseButton, PanAndZoom.MouseAction.Up, pointer.Position, worldPos);
                RaiseEvent(eventArgs);
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            PointerPoint pointer = e.GetCurrentPoint(this);
            PointerPointProperties props = pointer.Properties;
            Point worldPos = PixelToWorld(pointer.Position);

            if (panningInProgress) {
                PanMove(pointer.Position);
            }
            else {
                MouseEventArgs eventArgs = new MouseEventArgs(MouseActivityEvent, this, MouseButton.None, PanAndZoom.MouseAction.Up, pointer.Position, worldPos);
                RaiseEvent(eventArgs);
            }
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

            // Mark the event as handled
            e.Handled = true;
        }

        float ScaleFactor {
            get { return pixelPerMm * zoom; }
        }

        void CalculateWorldTransform()
        {
            double layoutScale = LayoutHelper.GetLayoutScale(this);  // ratio between logical and physical pixels.

            // Get size, midpoint of the window .
            Size sizeInPixels = this.Bounds.Size;  
            Point midpoint = new Point(sizeInPixels.Width / 2.0F, sizeInPixels.Height / 2.0F);

            // Calculate the world->window transform.
            float scaleFactor = ScaleFactor;
            xformWorldToLogPixel = Matrix.CreateTranslation(-centerPoint.X, -centerPoint.Y) * 
                                Matrix.CreateScale(scaleFactor, -scaleFactor) * 
                                Matrix.CreateTranslation(midpoint.X, midpoint.Y);

            // Invert it to get the window->world transform.
            xformLogPixelToWorld = xformWorldToLogPixel.Invert();

            // Calculate the world->physical pixels transform.
            xformWorldToPhysPixel = Matrix.CreateTranslation(-centerPoint.X, -centerPoint.Y) *
                                Matrix.CreateScale(scaleFactor * layoutScale, -scaleFactor * layoutScale) *
                                Matrix.CreateTranslation(midpoint.X * layoutScale, midpoint.Y * layoutScale);

            // Invert it to get the physical pixel->world transform.
            xformPhysPixelToWorld = xformWorldToPhysPixel.Invert();

            // Calculate the viewport in world coordinates.
            Point[] pts = { new Point(0.0F, (float)sizeInPixels.Height), new Point((float)sizeInPixels.Width, 0.0F) };
            Point pt0 = xformLogPixelToWorld.Transform(new Point(0.0, sizeInPixels.Height));
            Point pt1 = xformLogPixelToWorld.Transform(new Point(sizeInPixels.Width, 0.0));
            viewport = new Rect(pt0.X, pt0.Y, pt1.X - pt0.X, pt1.Y - pt0.Y);
        }

        // Transform rectangle from world to pixel coordinates. 
        public Rect WorldToPixel(Rect rectWorld)
        {
            Point pt0 = xformWorldToLogPixel.Transform(new Point(rectWorld.Left, rectWorld.Top));
            Point pt1 = xformWorldToLogPixel.Transform(new Point(rectWorld.Right, rectWorld.Bottom));
            
            // Note that Y's are reversed, so we reverse the rectangle to make the rect height always positive.
            return new Rect(new Point(pt0.X, pt1.Y), new Size(pt1.X - pt0.X, pt0.Y - pt1.Y));
        }

        // Transform rectangle from pixel to world coordinates. 
        public Rect PixelToWorld(Rect rectPixel)
        {
            Point pt0 = xformLogPixelToWorld.Transform(new Point(rectPixel.Left, rectPixel.Top));
            Point pt1 = xformLogPixelToWorld.Transform(new Point(rectPixel.Right, rectPixel.Bottom));

            // Note that Y's are reversed, so we reverse the rectangle to make the rect height always positive.
            return new Rect(new Point(pt0.X, pt1.Y), new Size(pt1.X - pt0.X, pt0.Y - pt1.Y));
        }

        // Transform one point from world to pixel coordinates. 
        public Point WorldToPixel(Point ptWorld)
        {
            return xformWorldToLogPixel.Transform(ptWorld);
        }

        // Transform one point from pixel to world coordinates. 
        public Point PixelToWorld(Point ptPixel)
        {
            return xformLogPixelToWorld.Transform(ptPixel);
        }

        // Transform distance from world to pixel coordinates. 
        public float WorldToPixelDistance(float distWorld)
        {
            // M11 is the X scale, which is the same as Y scale since we use uniform scaling.
            return (float) (distWorld * xformWorldToLogPixel.M11);  
        }

        // Transform distance from pixe to world coordinates. 
        public float PixelToWorldDistance(float distPixel)
        {
            // M11 is the X scale, which is the same as Y scale since we use uniform scaling.
            return (float)(distPixel * xformLogPixelToWorld.M11);
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

        void BeginPanning(Point pt, MouseButton endingButton)
        {
            panningInProgress = true;
            endPanningButton = endingButton;
            lastPanScrollPoint = pt;
            //this.Cursor = DragCursor;
            //DisableHoverTimer();
        }

        void EndPanning(Point pt)
        {
            panningInProgress = false;
            //this.Cursor = Cursors.Default;
        }

        void PanMove(Point pt)
        {
            Debug.Assert(panningInProgress);

            Point worldLastPan = PixelToWorld(lastPanScrollPoint);
            Point worldCurrentPan = PixelToWorld(pt);

            CenterPoint = new Point(centerPoint.X + worldLastPan.X - worldCurrentPan.X, centerPoint.Y + worldLastPan.Y - worldCurrentPan.Y);

            lastPanScrollPoint = pt;
        }

        void ViewportChanged()
        {
            CalculateWorldTransform();
            this.InvalidateVisual();
        }

        // Always be hittable, even if we don't draw anything. This is needed to get
        // mouse events on this control.
        bool ICustomHitTest.HitTest(Avalonia.Point point)
        {
            // You have to check bounds, or else you get hit testing outside the control bounds.
            Rect controlBounds = new Rect(Bounds.Size);
            return controlBounds.Contains(point);
        }

        private MouseButton MouseButtonFromPointerUpdate(PointerUpdateKind pointerUpdateKind)
        {
            switch (pointerUpdateKind) {
            case PointerUpdateKind.LeftButtonPressed:
            case PointerUpdateKind.LeftButtonReleased:
                return MouseButton.LeftButton;

            case PointerUpdateKind.MiddleButtonPressed:
            case PointerUpdateKind.MiddleButtonReleased:
                return MouseButton.MiddleButton;

            case PointerUpdateKind.RightButtonPressed:
            case PointerUpdateKind.RightButtonReleased:
                return MouseButton.RightButton;

            default:
                return MouseButton.Other;
            }
        }

        // Other is only used internally -- will never be sent in an event. None is used for events that don't have a button, such as mouse move.
        public enum MouseButton { None, LeftButton, RightButton, MiddleButton, Other }

        // Types of mouse actions.
        public enum MouseAction
        {
            Down,      // mouse button pressed down
            Move,      // mouse was moved
            Drag,      // mouse was dragged with a button down, occurs together with (and after) MouseMove if dragging enabled

            // When mouse button is released, exactly one of the follow three occurs.
            Up,        // mouse button released (dragging disabled) 
            DragEnd,   // mouse button released (if dragging enabled)
            Click,     // mouse button release after no/little movement 

            // If a drag is started, but the mouse is taken away before finishing, a DragCancel event occurs
            DragCancel,

            // Mouse hovers a certain length of time without moving
            Hover,
        }

        // Possible responses to a mouse down.
        public enum MouseDownResult
        {
            None,           // no special handling. May get click event when released, and Up when released. No dragging or panning will occur.
            SuppressClick,  // no click event will occur. Up event will still occur.
            BeginPanning,   // begin panning immediately. No Click or Drag events will occurs.
            ImmediateDrag,  // begin dragging immediately. No Click event will occur, Drag, DragEnd events will occurs.
            DelayedDrag     // if the mouse moves enough before release, begin dragging, otherwise a Click event occurs.
        }

        // The information sent with a mouse event. 
        // Note that MouseDownResult is an OUT -- it is set by the handler of the event to indicate how the mousedown is handled.
        public class MouseEventArgs: RoutedEventArgs
        {
            public MouseEventArgs(RoutedEvent? routedEvent, object? source, MouseButton button, MouseAction action, Point logicalPixelLocation, Point worldLocation)
                : base(routedEvent, source)
            {
                this.Button = button;
                this.Action = action;
                this.LogicalPixelLocation = logicalPixelLocation;
                this.WorldLocation = worldLocation;
                this.WorldDragStart = worldLocation;
                this.MouseDownResult = MouseDownResult.None;
            }

            public MouseButton Button;
            public MouseAction Action;
            public Point LogicalPixelLocation;      // location in logical pixels in the control
            public Point WorldLocation;             // location in world coordinates in the control.
            public Point WorldDragStart;            // For a drag event, where the dragging began
            public MouseDownResult MouseDownResult; // For a mouse down, how the mouse down is handled.
        }
    }
}
