using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class NewEventDirectoryPage : UserControl
    {
        public NewEventDirectoryPage() => InitializeComponent();

        private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
        {
            if (VisualRoot is NewEventWizardDialog wizard &&
                DataContext is NewEventDirectoryPageViewModel vm) {
                await wizard.BrowseDirectory(vm);
            }
        }
    }
}
