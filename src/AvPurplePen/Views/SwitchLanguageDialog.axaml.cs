// SwitchLanguageDialog.axaml.cs
//
// Code-behind for the language-switching dialog. Handles OK/Cancel
// button clicks and double-click on a language item, closing the
// dialog with a boolean result indicating whether the user confirmed.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Modal dialog that lets the user pick a UI language from a list.
    /// Returns true from ShowDialog if the user clicked OK, false otherwise.
    /// </summary>
    public partial class SwitchLanguageDialog : Window
    {
        /// <summary>
        /// Initializes the dialog and its components.
        /// </summary>
        public SwitchLanguageDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the OK button click. Closes the dialog returning true.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        /// <summary>
        /// Handles the Cancel button click. Closes the dialog returning false.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        /// <summary>
        /// Handles double-clicking a language item. Acts like clicking OK.
        /// </summary>
        private void LanguageListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            Close(true);
        }
    }
}
