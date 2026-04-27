// LegAssignmentsDialog.axaml.cs
//
// Code-behind for the Leg Assignments sub-dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    public partial class LegAssignmentsDialog : Window
    {
        public LegAssignmentsDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e) => Close(true);
        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}
