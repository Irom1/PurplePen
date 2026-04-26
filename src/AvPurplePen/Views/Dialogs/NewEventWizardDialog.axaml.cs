// NewEventWizardDialog.axaml.cs
//
// Code-behind for the New Event Wizard. Handles Back/Next/Cancel button clicks
// and file/folder picking on behalf of the page ViewModels (which cannot call
// Avalonia APIs directly).

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Multi-page wizard dialog for creating a new event.
    /// Close(true) is called when the user finishes successfully.
    /// </summary>
    public partial class NewEventWizardDialog : Window
    {
        public NewEventWizardDialog()
        {
            InitializeComponent();
        }

        private NewEventWizardDialogViewModel? Vm => DataContext as NewEventWizardDialogViewModel;

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            Vm?.GoBackCommand.Execute(null);
        }

        private async void NextButton_Click(object? sender, RoutedEventArgs e)
        {
            NewEventWizardDialogViewModel? vm = Vm;
            if (vm == null) return;

            // Map file page: browse when the user presses Next without a file loaded.
            if (vm.CurrentPage is NewEventMapFilePageViewModel mapPage && !mapPage.CanProceed) {
                await BrowseMapFile(mapPage);
                return;
            }

            // Directory page: browse if "other folder" is selected but no path set.
            if (vm.CurrentPage is NewEventDirectoryPageViewModel dirPage &&
                !dirPage.UseMapDirectory && string.IsNullOrWhiteSpace(dirPage.OtherDirectory)) {
                await BrowseDirectory(dirPage);
                return;
            }

            bool done = vm.TryGoNext();
            if (done)
                Close(true);
        }

        /// <summary>
        /// Opens the platform file picker for map file selection.
        /// Called by the Browse button on the map file page.
        /// </summary>
        internal async System.Threading.Tasks.Task BrowseMapFile(NewEventMapFilePageViewModel vm)
        {
            string? path = null;
            PurplePen.ViewModels.FileOpenSingleViewModel fileVm = new() {
                FileFilters = MiscText.OpenFileDialog_MapFilter,
                InitialFileFilterIndex = 1
            };
            bool ok = await Services.DialogService.ShowDialogAsync(fileVm);
            if (ok && fileVm.SelectedFile != null)
                path = fileVm.SelectedFile;

            if (path != null)
                vm.ValidateAndSetMapFile(path);
        }

        /// <summary>
        /// Opens the platform folder picker for save-directory selection.
        /// Called by the Browse button on the directory page.
        /// </summary>
        internal async System.Threading.Tasks.Task BrowseDirectory(NewEventDirectoryPageViewModel vm)
        {
            string? initial = string.IsNullOrWhiteSpace(vm.OtherDirectory)
                ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
                : vm.OtherDirectory;

            string? picked = await Services.DialogService.ShowFolderPickerAsync(initial);
            if (picked != null)
                vm.OtherDirectory = picked;
        }
    }
}
