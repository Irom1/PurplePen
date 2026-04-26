// IDialogService.cs
//
// Abstraction for showing modal dialogs from ViewModels.
// Lives in PurplePenCore so it can be accessed via Services.DialogService.
// The implementation lives in the View layer (AvPurplePen) so that
// ViewModels remain free of UI dependencies and are testable with mocks.

using System.Threading.Tasks;

namespace PurplePen
{
    /// <summary>
    /// Service for displaying modal dialogs from ViewModels.
    /// The caller creates and configures the dialog's ViewModel, then passes
    /// it to <see cref="ShowDialogAsync{TViewModel}"/>. After the dialog closes,
    /// the caller inspects the ViewModel's properties for results.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a modal dialog for the given ViewModel.
        /// The View is resolved automatically via the ViewLocator convention
        /// (FooViewModel → FooView).
        /// </summary>
        /// <typeparam name="TViewModel">The ViewModel type for the dialog.</typeparam>
        /// <param name="viewModel">The ViewModel instance, pre-configured by the caller.</param>
        /// <returns>True if the dialog was accepted (OK), false if cancelled.</returns>
        Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class;

        /// <summary>
        /// Shows a non-modal progress window. Call <see cref="CloseProgressWindow"/> when the
        /// operation completes. The View is resolved from the ViewModel type using the same
        /// naming convention as <see cref="ShowDialogAsync{TViewModel}"/>.
        /// </summary>
        void ShowProgressWindow<TViewModel>(TViewModel viewModel) where TViewModel : class;

        /// <summary>
        /// Closes the progress window previously shown by <see cref="ShowProgressWindow{TViewModel}"/>.
        /// </summary>
        void CloseProgressWindow();

        /// <summary>
        /// Shows the platform folder-picker dialog.
        /// </summary>
        /// <param name="initialDirectory">Optional starting directory path.</param>
        /// <returns>The selected folder path, or null if the user cancelled.</returns>
        Task<string?> ShowFolderPickerAsync(string? initialDirectory = null);
    }
}
