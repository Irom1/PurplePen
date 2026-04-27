// AddTextLineDialog.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    public partial class AddTextLineDialog : Window
    {
        public AddTextLineDialog()
        {
            InitializeComponent();
            Opened += (s, e) => textBoxText.Focus();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e) => Close(true);
        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}
