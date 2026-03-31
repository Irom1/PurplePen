using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvPurplePen.Views;
using AvUtil;
using PurplePen;

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
    private void panAndZoom_MouseActivity(object? sender, PanAndZoom.MouseEventArgs e)
    {
        if (e.Action == PanAndZoom.MouseAction.Down && 
            (e.Button == PanAndZoom.MouseButton.RightButton || e.Button == PanAndZoom.MouseButton.MiddleButton)) 
        {
            // Middle and right mouse buttons always pan the map.
            e.MouseDownResult = PanAndZoom.MouseDownResult.BeginPanning;
        }
    }
}