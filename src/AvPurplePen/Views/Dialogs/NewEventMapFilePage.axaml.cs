using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class NewEventMapFilePage : UserControl
    {
        public NewEventMapFilePage() => InitializeComponent();

        private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
        {
            // Delegate to wizard dialog so we can use its async infrastructure.
            if (VisualRoot is NewEventWizardDialog wizard &&
                DataContext is NewEventMapFilePageViewModel vm) {
                await wizard.BrowseMapFile(vm);
            }
        }
    }
}
