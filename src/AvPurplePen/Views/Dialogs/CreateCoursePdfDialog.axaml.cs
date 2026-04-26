// CreateCoursePdfDialog.axaml.cs
//
// Code-behind for the Create PDF Files dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class CreateCoursePdfDialog : Window
    {
        public CreateCoursePdfDialog()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateCoursePdfDialogViewModel vm = (CreateCoursePdfDialogViewModel)DataContext!;

            if (!vm.AllCoursesSelected && vm.GetSettings().CourseIds.Length == 0) {
                // No courses selected — handled by showing nothing (could add a message here).
                return;
            }

            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private async void BrowseFolderButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateCoursePdfDialogViewModel vm = (CreateCoursePdfDialogViewModel)DataContext!;
            string? folder = await Services.DialogService.ShowFolderPickerAsync(vm.OtherDirectory);
            if (folder != null)
                vm.OtherDirectory = folder;
        }
    }
}
