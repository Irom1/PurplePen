using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvUtil;
using PurplePen;

namespace AvPurplePen;

public partial class MapViewer : UserControl
{
    // Set the IMapDisplay that this map viewer should display. The map will be drawn
    // in a background thread and cached for better performance. Setting to null will clear the map.
    public static readonly StyledProperty<IMapDisplay?> MapDisplayProperty =
            AvaloniaProperty.Register<MapViewer, IMapDisplay?>(nameof(MapDisplay));

    public MapViewer()
    {
        InitializeComponent();
    }

    public IMapDisplay? MapDisplay {
        get => GetValue(MapDisplayProperty);
        set => SetValue(MapDisplayProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MapDisplayProperty) {
            // The map to display has changed. Create a new CacheableMapDisplay
            // for the new map and set it as the drawing for the pan and zoom control.

            IMapDisplay? mapDisplay = change.GetNewValue<IMapDisplay?>();
            if (mapDisplay != null) {
                IAsyncSkiaDrawing skiaDrawing = new CacheableMapDisplay(mapDisplay);
                panAndZoom.Drawing = new CachedDrawing(skiaDrawing);
            }
            else {
                panAndZoom.Drawing = null;
            }
        }
    }
}