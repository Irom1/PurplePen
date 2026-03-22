// FileOpenSingleViewModel.cs
//
// ViewModel for opening a single file via a platform file-open dialog.
// Contains the options needed to configure the dialog and receives the
// result (selected file path) after the dialog closes. Does not reference
// any platform-specific types.

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for a file-open dialog that selects a single file.
    /// Set the configuration properties before showing the dialog;
    /// after the dialog closes, read <see cref="SelectedFile"/> for the result.
    /// </summary>
    public class FileOpenSingleViewModel : ViewModelBase
    {
        /// <summary>
        /// The title bar text of the dialog, or null to use the platform default.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The initial directory to browse from, or null to use the platform default.
        /// </summary>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// A Windows-style file filter string, e.g. "Purple Pen files|*.ppen|All files|*.*".
        /// Each pair of segments (display name, pattern) is separated by '|'.
        /// </summary>
        public string FileFilters { get; set; } = "";

        /// <summary>
        /// A 1-based index into <see cref="FileFilters"/> indicating which filter
        /// is initially active.
        /// </summary>
        public int InitialFileFilterIndex { get; set; } = 1;

        /// <summary>
        /// After the dialog closes, the full path of the selected file,
        /// or null if the user cancelled.
        /// </summary>
        public string? SelectedFile { get; set; }
    }
}
