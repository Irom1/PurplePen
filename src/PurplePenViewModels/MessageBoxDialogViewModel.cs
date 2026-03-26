// MessageBoxViewModel.cs
//
// ViewModel for a standard message box dialog. Specifies the message text,
// which buttons to show (OK, OK/Cancel, Yes/No, Yes/No/Cancel), which button
// is the default, and an icon style. After the dialog closes, ChosenButton
// indicates which button the user clicked.
//
// This ViewModel contains no Avalonia types — the View layer translates
// the enum values into the appropriate UI elements.

namespace PurplePen.ViewModels
{
    /// <summary>
    /// The set of buttons to display in the message box.
    /// </summary>
    public enum MessageBoxButtons
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// Identifies a single button in the message box.
    /// Used for both <see cref="MessageBoxDialogViewModel.DefaultButton"/>
    /// and <see cref="MessageBoxDialogViewModel.ChosenButton"/>.
    /// </summary>
    public enum MessageBoxButton
    {
        None,
        Ok,
        Cancel,
        Yes,
        No
    }

    /// <summary>
    /// The icon style shown beside the message text.
    /// </summary>
    public enum MessageBoxIcon
    {
        None,
        Information,
        Warning,
        Error,
        Question
    }

    /// <summary>
    /// ViewModel for a standard message box dialog.
    /// Set the properties before showing the dialog via IDialogService.
    /// After the dialog closes, read <see cref="ChosenButton"/> to determine
    /// the user's response.
    /// </summary>
    public class MessageBoxDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// The message text to display.
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// The title bar text, or null to use the application default.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Which set of buttons to display.
        /// </summary>
        public MessageBoxButtons Buttons { get; set; } = MessageBoxButtons.Ok;

        /// <summary>
        /// Which button is the default (has initial focus and responds to Enter).
        /// </summary>
        public MessageBoxButton DefaultButton { get; set; } = MessageBoxButton.Ok;

        /// <summary>
        /// The icon to display beside the message.
        /// </summary>
        public MessageBoxIcon Icon { get; set; } = MessageBoxIcon.None;

        /// <summary>
        /// After the dialog closes, indicates which button the user clicked.
        /// Set by the View before closing.
        /// </summary>
        public MessageBoxButton ChosenButton { get; set; } = MessageBoxButton.None;
    }
}
