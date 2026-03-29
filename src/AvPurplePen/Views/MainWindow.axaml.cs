// MainWindow.axaml.cs
//
// Code-behind for the main window. Handles UI events that need
// direct window interaction (like showing modal dialogs), which
// don't fit cleanly into the ViewModel layer.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

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
            ApplicationIdleService.ApplicationIdle += ApplicationIdle;
            descriptionViewer.Change += DescriptionViewer_Change;
            descriptionViewer.SelectedIndexChange += DescriptionViewer_SelectedIndexChange;
        }

        // Event fired when the user clicks on a line to select a new line in the description control.
        private void DescriptionViewer_SelectedIndexChange(object? sender, EventArgs e)
        {
            if (this.DataContext is MainWindowViewModel viewModel) {
                // The user has selected a new line in the description. Update the view model to match.
                viewModel.DescriptionSelection = descriptionViewer.Selection;
            }
        }

        // Event fired when the user changes something in the description control.
        private void DescriptionViewer_Change(object sender, DescriptionChangeKind kind, int line, int box, object newValue)
        {
#if !PORTING
            //TODO: Implemented this.
#endif
        }

        // This is called when the application becomes idle after processing input. We can use this to update
        // the UI in response to changes that may have occurred.
        private void ApplicationIdle(object? sender, System.EventArgs e)
        {
            if (this.IsVisible) {
                // The application is idle. If the application state has changed, update the
                // user interface to match.
                if (this.DataContext is MainWindowViewModel viewModel) {
                    viewModel.UpdateStateOnIdle();
                }
            }
        }
    }
}
