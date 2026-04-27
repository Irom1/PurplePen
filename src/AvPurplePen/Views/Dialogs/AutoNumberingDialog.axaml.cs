// AutoNumberingDialog.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    public partial class AutoNumberingDialog : Window
    {
        public AutoNumberingDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e) => Close(true);
        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}
