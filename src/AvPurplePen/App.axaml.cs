using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvPurplePen.Views;
using System.Linq;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen
{
    public partial class App : Application
    {
        /// <summary>
        /// The main application window. Set during initialization and used by
        /// the IDialogService factory to create modal dialogs.
        /// </summary>
        public static Window? MainWindow { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();


                MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
                Controller controller = new Controller(mainWindowViewModel);

                MainWindow mainWindow = new MainWindow {
                    DataContext = mainWindowViewModel,
                };
                desktop.MainWindow = mainWindow;
                App.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove) {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}