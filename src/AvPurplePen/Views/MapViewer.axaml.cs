using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvPurplePen.Views;
using AvUtil;
using PurplePen;
using System;

namespace AvPurplePen;

public partial class MapViewer : UserControl
{
    // Set the IMapDisplay that this map viewer should display. The map will be drawn
    // in a background thread and cached for better performance. Setting to null will clear the map.
    public static readonly StyledProperty<IMapDisplay?> MapDisplayProperty =
            AvaloniaProperty.Register<MapViewer, IMapDisplay?>(nameof(MapDisplay));

    // Has the map highlights that this map viewer should display.
    public static readonly StyledProperty<IMapViewerHighlight[]?> MapHighlightsProperty =
            AvaloniaProperty.Register<MainWindow, IMapViewerHighlight[]?>(nameof(MapHighlights));

    public static readonly RoutedEvent<FancyMouseEventArgs> FancyMouseActivityEvent =
        RoutedEvent.Register<MapViewer, FancyMouseEventArgs>(
            name: nameof(FancyMouseActivity),
            routingStrategy: RoutingStrategies.Direct);


    private HighlightDrawing highlightDrawing = new HighlightDrawing();

    public MapViewer()
    {
        InitializeComponent();
    }

    public IMapDisplay? MapDisplay {
        get => GetValue(MapDisplayProperty);
        set => SetValue(MapDisplayProperty, value);
    }

    public IMapViewerHighlight[]? MapHighlights {
        get => GetValue(MapHighlightsProperty);
        set => SetValue(MapHighlightsProperty, value);
    }

    public event EventHandler<FancyMouseEventArgs> FancyMouseActivity {
        add => AddHandler(FancyMouseActivityEvent, value);
        remove => RemoveHandler(FancyMouseActivityEvent, value);
    }

    public float PixelSize {
        get {
            return panAndZoom.PixelToWorldDistance(1);
        }
    }


    private void MapDisplayChanged(IMapDisplay? newMapDisplay)
    {
        // The map to display has changed. Create a new CacheableMapDisplay
        // for the new map and set it as the drawing for the pan and zoom control.

        if (newMapDisplay != null) {
            // The PanAndZoom control should display the merging of the map and the highlights.
            IAsyncSkiaDrawing skiaDrawing = new CacheableMapDisplay(newMapDisplay);
            IAvaloniaDrawing mapDrawing = new CachedDrawing(skiaDrawing);
            IAvaloniaDrawing mergedDrawing = new AvaloniaDrawingMerge(mapDrawing, highlightDrawing);

            panAndZoom.Drawing = mergedDrawing;
        }
        else {
            panAndZoom.Drawing = null;
        }
    }

    private void HighlightsChanged(IMapViewerHighlight[]? newMapHighlights)
    {
        // The highlights to display have changed. Update the highlight drawing.
        // This will automatically cause a redraw.
        highlightDrawing.SetHighlights(newMapHighlights);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MapDisplayProperty) {
            IMapDisplay? newMapDisplay = change.GetNewValue<IMapDisplay?>();
            MapDisplayChanged(newMapDisplay);
        }
        else if (change.Property == MapHighlightsProperty) {
            IMapViewerHighlight[]? newMapHighlights = change.GetNewValue<IMapViewerHighlight[]?>();
            HighlightsChanged(newMapHighlights);
        }
    }

    // A mouse event has occurred.
    private void panAndZoom_MouseActivity(object? sender, PanAndZoom.BasicMouseEventArgs e)
    {
        if (e.BasicAction == PanAndZoom.BasicMouseAction.Down && 
            (e.Button == MouseButton.Right || e.Button == MouseButton.Middle)) 
        {
            // Middle and right mouse buttons always pan the map.
            panAndZoom.BeginPanning(e.LogicalPixelLocation, e.Button);
        }
        else {
            // Reraise the event to the main window.

            // Temporary: this is just to get things to compile. Eventually we want translate basic
            // mouse up/down/move into clicks, drags, hovers, etc.

            FancyMouseAction fancyAction;
            switch (e.BasicAction) {
            case PanAndZoom.BasicMouseAction.Down:
                fancyAction = FancyMouseAction.Down;
                break;
            case PanAndZoom.BasicMouseAction.Move:
                fancyAction = FancyMouseAction.Move;
                break;
            case PanAndZoom.BasicMouseAction.Up:
                fancyAction = FancyMouseAction.Up;
                break;
            default:
                return;
            }

            FancyMouseEventArgs args =
                new FancyMouseEventArgs(FancyMouseActivityEvent, this, e.Button, fancyAction, e.WorldLocation);
            RaiseEvent(args);
        }
    }

    // Types of mouse actions.
    public enum FancyMouseAction
    {
        Down,      // mouse button pressed down
        Move,      // mouse was moved
        Drag,      // mouse was dragged with a button down, occurs together with (and after) MouseMove
                   // if ImmediateDrag or DelayedDrag was returned from a Mouse Down

        // When mouse button is released, exactly one of the follow three occurs.
        Up,        // mouse button released (dragging disabled) 
        DragEnd,   // mouse button released (if dragging enabled)
        Click,     // mouse button release after no/little movement, and a short amount of time down. 

        // If a drag is started, but the mouse is taken away before finishing, a DragCancel event occurs
        DragCancel,

        // Mouse hovers a certain length of time without moving
        Hover,
    }

    // Possible responses to a mouse down. Allows the received to decide if the mouse down should
    // possibly begin a drag or pan, or just handled as a click, or to suppress clicks.
    public enum MouseDownResult
    {
        None,           // no special handling. May get click event when released, and Up when released. No dragging or panning will occur.
        SuppressClick,  // no click event will occur. Up event will still occur.
        ImmediatePan,   // begin panning immediately. No Click or Drag events will occurs.
        DelayedPan,     // if the mouse moves enough before release, begin panning, otherwise a Click event occurs.
        ImmediateDrag,  // begin dragging immediately. No Click event will occur, Drag, DragEnd events will occurs.
        DelayedDrag     // if the mouse moves enough before release, begin dragging, otherwise a Click event occurs.
    }


    // The information sent with a mouse event. 
    // Note that MouseDownResult is a response to a mouse down event,
    // that is set by the receiver of the event to tell the MapViewer how to handle the mouse down.
    public class FancyMouseEventArgs : RoutedEventArgs
    {
        public FancyMouseEventArgs(RoutedEvent? routedEvent, object? source, MouseButton button, FancyMouseAction action, Point worldLocation)
            : base(routedEvent, source)
        {
            this.Button = button;
            this.FancyAction = action;
            this.WorldLocation = worldLocation;
        }

        public MouseButton Button;              // Not used for a Move action.
        public FancyMouseAction FancyAction;    // Fancy mouse action: includes, drags, clicks, hovers.
        public Point WorldLocation;             // location in world coordinates in the control.
        public Point WorldDragStart;            // For a drag event, where the dragging began
        public MouseDownResult MouseDownResult; // For a mouse down, how the mouse down is handled.
    }

}