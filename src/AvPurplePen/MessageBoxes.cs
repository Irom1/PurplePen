// MessageBoxes.cs
//
// Utility methods for showing message box dialogs in Avalonia.
// Uses simple child windows since Avalonia doesn't have built-in
// MessageBox support. Replaces WinForms MessageBox.Show() calls.

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AvPurplePen
{
    /// <summary>
    /// Provides helper methods for showing standard message box dialogs.
    /// </summary>
    public static class MessageBoxes
    {
        /// <summary>
        /// Shows an error message box with an OK button.
        /// </summary>
        /// <param name="owner">The parent window.</param>
        /// <param name="message">The error message to display.</param>
        public static async Task ShowErrorAsync(Window owner, string message)
        {
            Window msgBox = new Window {
                Title = PurplePen.MiscText.AppTitle,
                Width = 380,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                CanMinimize = false,
                CanMaximize = false,
                Icon = owner.Icon,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,
                Content = new StackPanel {
                    Margin = new Thickness(20),
                    Spacing = 16,
                    Children = {
                        new TextBlock {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        },
                        new Button {
                            Content = "OK",
                            Width = 85,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            HorizontalContentAlignment = HorizontalAlignment.Center,
                            IsDefault = true,
                        }
                    }
                }
            };

            // Wire up OK button to close.
            StackPanel panel = (StackPanel)msgBox.Content;
            Button okBtn = (Button)panel.Children[1];
            okBtn.Click += (s, e) => msgBox.Close();

            await msgBox.ShowDialog(owner);
        }
    }
}
