// MapIssueChoiceDialog.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class MapIssueChoiceDialog : Window
    {
        public MapIssueChoiceDialog()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MapIssueChoiceDialogViewModel vm)
                vm.SelectedKind = MapIssueKind.Beginning;
            Close(true);
        }

        private void MiddleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MapIssueChoiceDialogViewModel vm)
                vm.SelectedKind = MapIssueKind.Middle;
            Close(true);
        }

        private void StartTriangleButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MapIssueChoiceDialogViewModel vm)
                vm.SelectedKind = MapIssueKind.End;
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}
