// MessageBoxDialog.axaml.cs
//
// Code-behind for the message box dialog. Configures button visibility,
// default button, icon, and title based on the MessageBoxViewModel
// properties. The ViewModel's ChosenButton is set before closing.

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Standard message box dialog with configurable buttons, icon, and default button.
    /// The caller must set DataContext to a <see cref="MessageBoxDialogViewModel"/> before showing.
    /// </summary>
    public partial class MessageBoxDialog : Window
    {
        // SVG path data for standard message box icons (from Material Design Icons).
        private const string InformationIconData = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M13,17V11H11V17H13M13,9V7H11V9H13Z";
        private const string WarningIconData = "M1,21H23L12,2L1,21M13,18H11V16H13V18M13,14H11V10H13V14Z";
        private const string ErrorIconData = "M12,2A10,10 0 0,1 22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2M13,13V7H11V13H13M13,17V15H11V17H13Z";
        private const string QuestionIconData = "M12,2C17.52,2 22,6.48 22,12C22,17.52 17.52,22 12,22C6.48,22 2,17.52 2,12C2,6.48 6.48,2 12,2ZM12,15.5C11.45,15.5 11,15.95 11,16.5C11,17.05 11.45,17.5 12,17.5C12.55,17.5 13,17.05 13,16.5C13,15.95 12.55,15.5 12,15.5ZM12,5.5C9.72,5.5 8,7.22 8,9.5H9.5C9.5,8.12 10.62,7 12,7C13.38,7 14.5,8.12 14.5,9.5C14.5,10.46 13.97,11.31 13.14,11.72L12.84,11.86C11.79,12.33 11.25,13.3 11.25,14.5H12.75C12.75,13.67 13.1,13.22 13.73,12.9L14.03,12.77C15.25,12.15 16,10.91 16,9.5C16,7.22 14.28,5.5 12,5.5Z";

        /// <summary>
        /// Initializes the dialog and its components.
        /// </summary>
        public MessageBoxDialog()
        {
            InitializeComponent();
            Opened += MessageBoxDialog_Opened;
        }

        /// <summary>
        /// Configures buttons, icon, title, and default focus when the dialog opens.
        /// </summary>
        private void MessageBoxDialog_Opened(object? sender, System.EventArgs e)
        {
            MessageBoxDialogViewModel? vm = DataContext as MessageBoxDialogViewModel;
            if (vm == null)
                return;

            // Set the title.
            if (vm.Title != null) {
                Title = vm.Title;
            }
            else {
                Title = MiscText.AppTitle;
            }

            // Configure button visibility based on the button set.
            switch (vm.Buttons) {
                case MessageBoxButtons.Ok:
                    okButton.IsVisible = true;
                    break;
                case MessageBoxButtons.OkCancel:
                    okButton.IsVisible = true;
                    cancelButton.IsVisible = true;
                    break;
                case MessageBoxButtons.YesNo:
                    yesButton.IsVisible = true;
                    noButton.IsVisible = true;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    yesButton.IsVisible = true;
                    noButton.IsVisible = true;
                    cancelButton.IsVisible = true;
                    break;
            }

            // Configure IsDefault and IsCancel on the appropriate buttons.
            switch (vm.DefaultButton) {
                case MessageBoxButton.Ok:
                    okButton.IsDefault = true;
                    break;
                case MessageBoxButton.Cancel:
                    cancelButton.IsDefault = true;
                    break;
                case MessageBoxButton.Yes:
                    yesButton.IsDefault = true;
                    break;
                case MessageBoxButton.No:
                    noButton.IsDefault = true;
                    break;
            }

            // Cancel button always responds to Escape.
            if (cancelButton.IsVisible) {
                cancelButton.IsCancel = true;
            }
            else if (noButton.IsVisible) {
                noButton.IsCancel = true;
            }
            else {
                okButton.IsCancel = true;
            }

            // Configure the icon.
            ConfigureIcon(vm.Icon);
        }

        /// <summary>
        /// Sets the icon path data and fill color based on the icon style.
        /// </summary>
        private void ConfigureIcon(MessageBoxIcon icon)
        {
            string? pathData = null;
            IBrush fill = Brushes.Gray;

            switch (icon) {
                case MessageBoxIcon.Information:
                    pathData = InformationIconData;
                    fill = Brushes.DodgerBlue;
                    break;
                case MessageBoxIcon.Warning:
                    pathData = WarningIconData;
                    fill = Brushes.Orange;
                    break;
                case MessageBoxIcon.Error:
                    pathData = ErrorIconData;
                    fill = Brushes.Red;
                    break;
                case MessageBoxIcon.Question:
                    pathData = QuestionIconData;
                    fill = Brushes.DodgerBlue;
                    break;
            }

            if (pathData != null) {
                iconPath.Data = Geometry.Parse(pathData);
                iconPath.Fill = fill;
                iconViewbox.IsVisible = true;
            }
        }

        /// <summary>
        /// Handles the OK button click.
        /// </summary>
        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            SetChosenButtonAndClose(MessageBoxButton.Ok);
        }

        /// <summary>
        /// Handles the Yes button click.
        /// </summary>
        private void YesButton_Click(object? sender, RoutedEventArgs e)
        {
            SetChosenButtonAndClose(MessageBoxButton.Yes);
        }

        /// <summary>
        /// Handles the No button click.
        /// </summary>
        private void NoButton_Click(object? sender, RoutedEventArgs e)
        {
            SetChosenButtonAndClose(MessageBoxButton.No);
        }

        /// <summary>
        /// Handles the Cancel button click.
        /// </summary>
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            SetChosenButtonAndClose(MessageBoxButton.Cancel);
        }

        /// <summary>
        /// Sets the ChosenButton on the ViewModel and closes the dialog.
        /// Returns true for affirmative buttons (OK, Yes) and false otherwise.
        /// </summary>
        private void SetChosenButtonAndClose(MessageBoxButton button)
        {
            MessageBoxDialogViewModel? vm = DataContext as MessageBoxDialogViewModel;
            if (vm != null) {
                vm.ChosenButton = button;
            }

            bool isAffirmative = (button == MessageBoxButton.Ok || button == MessageBoxButton.Yes);
            Close(isAffirmative);
        }
    }
}
