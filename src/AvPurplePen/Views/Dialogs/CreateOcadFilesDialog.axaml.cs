// CreateOcadFilesDialog.axaml.cs
//
// Code-behind for the Create OCAD Files dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class CreateOcadFilesDialog : Window
    {
        public CreateOcadFilesDialog()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private async void BrowseFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateOcadFilesDialogViewModel vm = (CreateOcadFilesDialogViewModel)DataContext!;
            string? folder = await Services.DialogService.ShowFolderPickerAsync(vm.OtherDirectory);
            if (folder != null)
                vm.OtherDirectory = folder;
        }
    }
}
