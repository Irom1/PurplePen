using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvUtil;
using PurplePen.MapModel;
using System.Diagnostics;
using System.IO;

namespace AvMapViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object? sender, RoutedEventArgs e)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            TopLevel topLevel = TopLevel.GetTopLevel(this)!;

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = "Open Map File",
                AllowMultiple = false
            });

            if (files.Count >= 1) {
                string? path = files[0].TryGetLocalPath();
                Debug.WriteLine($"Selected file: {path}");
                Map map = new Map(new Skia_TextMetrics(), new Skia_FileLoader(Path.GetDirectoryName(path)!));
                InputOutput.ReadFile(path, map);

                CacheableMapDrawing cacheableMapDrawing = new CacheableMapDrawing();
                cacheableMapDrawing.Map = map;
                panAndZoom.Drawing = new CachedDrawing(cacheableMapDrawing);
            }
        }
    }
}