using Avalonia.Controls;
namespace AvPurplePen.Views
{
    public partial class NewEventTitlePage : UserControl
    {
        public NewEventTitlePage()
        {
            InitializeComponent();
            Loaded += (s, e) => titleTextBox.Focus();
        }
    }
}
