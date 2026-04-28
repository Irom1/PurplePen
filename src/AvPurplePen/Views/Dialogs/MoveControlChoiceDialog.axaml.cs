// MoveControlChoiceDialog.axaml.cs
//
// Code-behind for the Move Control Choice dialog.
// The three choice buttons each set ChosenResult on the ViewModel before closing.
//
// Migrated from WinForms PurplePen/MoveControlChoiceDialog.cs.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Dialog shown when a user drags a shared control, offering three choices:
    /// move in all courses, create a new control in this course, or cancel.
    /// The caller must set DataContext to a MoveControlChoiceDialogViewModel before showing.
    /// </summary>
    public partial class MoveControlChoiceDialog : Window
    {
        /// <summary>
        /// Initializes the dialog and sets initial focus to the first choice button.
        /// </summary>
        public MoveControlChoiceDialog()
        {
            InitializeComponent();
            Opened += (s, e) => moveButton.Focus();
        }

        /// <summary>
        /// Moves the control in all courses and closes the dialog.
        /// </summary>
        private void MoveButton_Click(object? sender, RoutedEventArgs e)
        {
            SetResultAndClose(YesNoCancel.Yes);
        }

        /// <summary>
        /// Creates a new control in this course only and closes the dialog.
        /// </summary>
        private void DuplicateButton_Click(object? sender, RoutedEventArgs e)
        {
            SetResultAndClose(YesNoCancel.No);
        }

        /// <summary>
        /// Cancels the move operation and closes the dialog.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            SetResultAndClose(YesNoCancel.Cancel);
        }

        /// <summary>
        /// Stores the chosen result on the ViewModel and closes the dialog.
        /// Returns true for actionable choices (Yes/No) and false for Cancel.
        /// </summary>
        private void SetResultAndClose(YesNoCancel result)
        {
            MoveControlChoiceDialogViewModel? vm = DataContext as MoveControlChoiceDialogViewModel;
            if (vm != null) {
                vm.ChosenResult = result;
            }

            Close(result != YesNoCancel.Cancel);
        }
    }
}
