// DialogService.cs
//
// Implementation of IDialogService for Avalonia.
// Uses the ViewLocator convention to resolve ViewModel types to View types,
// then shows the View as a modal dialog.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PurplePen;

namespace AvPurplePen
{
    /// <summary>
    /// Shows modal dialogs by resolving Views from ViewModels using the
    /// same naming convention as <see cref="ViewLocator"/>:
    /// PurplePen.ViewModels.FooViewModel → AvPurplePen.Views.FooView.
    /// </summary>
    [RequiresUnreferencedCode("DialogService uses reflection to resolve View types from ViewModel types.")]
    public class DialogService : IDialogService
    {
        private static readonly Assembly ViewAssembly = typeof(DialogService).Assembly;

        private readonly Window ownerWindow;

        /// <summary>
        /// Creates a new DialogService that shows dialogs owned by the given window.
        /// </summary>
        /// <param name="ownerWindow">The parent window for modal dialogs.</param>
        public DialogService(Window ownerWindow)
        {
            this.ownerWindow = ownerWindow;
        }

        /// <summary>
        /// Shows a modal dialog for the given ViewModel.
        /// The View is resolved via the naming convention FooViewModel → FooView.
        /// The ViewModel is set as the View's DataContext before showing.
        /// </summary>
        public async Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            // Resolve the View type from the ViewModel type using the same convention as ViewLocator.
            string viewModelName = typeof(TViewModel).FullName!;
            string viewName = viewModelName
                .Replace("PurplePen.ViewModels", "AvPurplePen.Views", StringComparison.Ordinal)
                .Replace("ViewModel", "", StringComparison.Ordinal);

            Type? viewType = ViewAssembly.GetType(viewName);
            if (viewType == null) {
                throw new InvalidOperationException($"Could not find View type '{viewName}' for ViewModel '{viewModelName}'.");
            }

            if (Activator.CreateInstance(viewType) is not Window dialog) {
                throw new InvalidOperationException($"View type '{viewName}' is not a Window.");
            }

            dialog.DataContext = viewModel;
            bool? result = await dialog.ShowDialog<bool?>(ownerWindow);
            return result == true;
        }

        /// <summary>
        /// Shows a file or folder picker dialog.
        /// Dispatches to the appropriate Avalonia StorageProvider method based on the options type.
        /// </summary>
        public async Task<string?> ShowFileDialogAsync<TOptions>(TOptions options) where TOptions : class
        {
            IStorageProvider storage = ownerWindow.StorageProvider;

            if (options is FilePickerOpenOptions openOptions) {
                var files = await storage.OpenFilePickerAsync(openOptions);
                return files.FirstOrDefault()?.Path.LocalPath;
            }

            if (options is FilePickerSaveOptions saveOptions) {
                IStorageFile? file = await storage.SaveFilePickerAsync(saveOptions);
                return file?.Path.LocalPath;
            }

            if (options is FolderPickerOpenOptions folderOptions) {
                var folders = await storage.OpenFolderPickerAsync(folderOptions);
                return folders.FirstOrDefault()?.Path.LocalPath;
            }

            throw new ArgumentException($"Unsupported file dialog options type: {typeof(TOptions).Name}. " +
                "Expected FilePickerOpenOptions, FilePickerSaveOptions, or FolderPickerOpenOptions.");
        }
    }
}
