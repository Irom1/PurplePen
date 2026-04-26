// ProgressDialog.axaml.cs
// Code-behind for the non-modal progress window. No logic here — the
// ProgressDialogViewModel owns all state.

using Avalonia.Controls;

namespace AvPurplePen.Views
{
    /// <summary>
    /// Non-modal progress window. Opened by DialogService.ShowProgressWindow and
    /// closed by DialogService.CloseProgressWindow.
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }
    }
}
