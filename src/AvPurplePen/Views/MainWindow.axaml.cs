// MainWindow.axaml.cs
//
// Code-behind for the main window. Handles UI events that need
// direct window interaction (like showing modal dialogs), which
// don't fit cleanly into the ViewModel layer.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// The main application window.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes the main window and its components.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens the Switch Language dialog modally with test data.
        /// </summary>
        private async void SwitchLanguageButton_Click(object? sender, RoutedEventArgs e)
        {
            SwitchLanguageViewModel viewModel = SwitchLanguageViewModel.CreateTestData();

            SwitchLanguageDialog dialog = new SwitchLanguageDialog {
                DataContext = viewModel,
            };

            bool? result = await dialog.ShowDialog<bool?>(this);

            if (result == true && viewModel.SelectedLanguage != null) {
                // For now, just update the window title to show the selection worked.
                Title = $"Selected: {viewModel.SelectedLanguage.DisplayName} ({viewModel.SelectedLanguage.Code})";
            }
        }
    }
}
