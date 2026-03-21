// MainWindow.axaml.cs
//
// Code-behind for the main window. Handles UI events that need
// direct window interaction (like showing modal dialogs), which
// don't fit cleanly into the ViewModel layer.

using System.Globalization;
using System.Linq;
using System.Threading;
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
            viewModel.SelectedLanguage = viewModel.AvailableLanguages.FirstOrDefault(lang => lang.Code == Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

            SwitchLanguageDialog dialog = new SwitchLanguageDialog {
                DataContext = viewModel,
            };

            bool? result = await dialog.ShowDialog<bool?>(this);

            if (result == true && viewModel.SelectedLanguage != null) {
                // Switch the UI culture and notify all LocalizeExtension bindings to refresh.
                CultureInfo newCulture = new CultureInfo(viewModel.SelectedLanguage.Code);
                Thread.CurrentThread.CurrentUICulture = newCulture;
                CultureInfo.DefaultThreadCurrentUICulture = newCulture;
                LocalizedStringManager.Instance.NotifyLanguageChanged();
            }
        }

        /// <summary>
        /// Opens the About dialog modally with a new AboutDialogViewModel.
        /// </summary>
        private async void AboutButton_Click(object? sender, RoutedEventArgs e)
        {
            AboutDialog dialog = new AboutDialog {
                DataContext = new AboutDialogViewModel(),
            };
            await dialog.ShowDialog(this);
        }

        /// <summary>
        /// Opens the Add Course dialog modally with test data.
        /// </summary>
        private async void AddCourseButton_Click(object? sender, RoutedEventArgs e)
        {
            AddCourseDialogViewModel viewModel = new AddCourseDialogViewModel();
            viewModel.InitializePrintScales(15000);
            viewModel.PrintScale = 10000;
            viewModel.Length = null;

            AddCourseDialog dialog = new AddCourseDialog {
                DataContext = viewModel,
            };

            bool? result = await dialog.ShowDialog<bool?>(this);

            if (result == true) {
                Title = $"Course: {viewModel.CourseName}, Scale: 1:{viewModel.PrintScale}";
            }
        }
    }
}
