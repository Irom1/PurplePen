// SetPrintAreaDialog.axaml.cs
//
// Code-behind for the Set Print Area dialog.
// Handles Done/Cancel button clicks, delegating controller interaction
// to the ViewModel.
//
// Migrated from WinForms PurplePen/SetPrintAreaDialog.cs.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog for setting the print area rectangle for a course or all courses.
    /// The caller must set DataContext to a SetPrintAreaDialogViewModel before showing.
    /// </summary>
    public partial class SetPrintAreaDialog : Window
    {
        /// <summary>
        /// Initializes the dialog and its components.
        /// </summary>
        public SetPrintAreaDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Commits the print area to the event database and closes the dialog.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            SetPrintAreaDialogViewModel? vm = DataContext as SetPrintAreaDialogViewModel;
            vm?.OnOk();
            Close(true);
        }

        /// <summary>
        /// Cancels the print area operation and closes the dialog.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            SetPrintAreaDialogViewModel? vm = DataContext as SetPrintAreaDialogViewModel;
            vm?.OnCancel();
            Close(false);
        }
    }
}
