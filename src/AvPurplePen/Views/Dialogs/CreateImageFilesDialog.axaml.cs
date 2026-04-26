// CreateImageFilesDialog.axaml.cs
//
// Code-behind for the Create Image Files dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class CreateImageFilesDialog : Window
    {
        public CreateImageFilesDialog()
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
            CreateImageFilesDialogViewModel vm = (CreateImageFilesDialogViewModel)DataContext!;
            string? folder = await Services.DialogService.ShowFolderPickerAsync(vm.OtherDirectory);
            if (folder != null)
                vm.OtherDirectory = folder;
        }
    }
}
