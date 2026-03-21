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

    }
}
