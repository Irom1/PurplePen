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
    }
}
