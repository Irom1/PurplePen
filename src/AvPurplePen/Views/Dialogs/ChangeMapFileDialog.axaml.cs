// ChangeMapFileDialog.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PurplePen.ViewModels;
using System.IO;

namespace AvPurplePen.Views
{
    public partial class ChangeMapFileDialog : Window
    {
        public ChangeMapFileDialog()
        {
            InitializeComponent();
        }

        private async void ChooseFileButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not ChangeMapFileDialogViewModel vm)
                return;

            string currentFile = vm.MapFile ?? "";
            string currentDir = !string.IsNullOrEmpty(currentFile)
                ? Path.GetDirectoryName(currentFile) ?? ""
                : "";

            FilePickerOpenOptions options = new FilePickerOpenOptions {
                Title = "Choose Map File",
                AllowMultiple = false,
                FileTypeFilter = DialogService.ParseFileFilters(
                    "All map files|*.ocd;*.omap;*.xmap;*.pdf;*.jpeg;*.jpg;*.tiff;*.tif;*.bmp;*.png;*.gif" +
                    "|OCAD files (*.ocd)|*.ocd" +
                    "|Open Orienteering Mapper Files|*.omap;*.xmap" +
                    "|PDF files (*.pdf)|*.pdf" +
                    "|Image files|*.jpeg;*.jpg;*.tiff;*.tif;*.bmp;*.png;*.gif"),
            };
            if (!string.IsNullOrEmpty(currentDir))
                options.SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(currentDir);

            System.Collections.Generic.IReadOnlyList<IStorageFile> files =
                await StorageProvider.OpenFilePickerAsync(options);
            if (files.Count > 0)
                vm.MapFile = files[0].Path.LocalPath;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e) => Close(true);
        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}
