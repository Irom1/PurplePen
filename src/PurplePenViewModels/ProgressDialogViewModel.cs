// ProgressDialogViewModel.cs
//
// ViewModel for the non-modal progress window shown during long-running operations
// such as PDF export and bitmap creation. The Controller calls ShowProgressDialog /
// UpdateProgressDialog / EndProgressDialog on IUserInterface; those methods drive
// this ViewModel.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the progress window shown during long-running operations.
    /// Set <see cref="IsIndeterminate"/> to true when total duration is unknown.
    /// </summary>
    public partial class ProgressDialogViewModel : ViewModelBase
    {
        /// <summary>Current status description shown to the user.</summary>
        [ObservableProperty]
        private string statusText = "";

        /// <summary>Progress from 0.0 to 1.0. Only used when IsIndeterminate is false.</summary>
        [ObservableProperty]
        private double fractionDone = 0.0;

        /// <summary>True while the total duration is unknown (indeterminate progress bar).</summary>
        [ObservableProperty]
        private bool isIndeterminate = true;

        /// <summary>True if a cancel action was provided and the Cancel button should be enabled.</summary>
        [ObservableProperty]
        private bool canCancel = false;

        private Action? onCancelPressed;

        /// <summary>
        /// Provides the action to invoke when the user clicks Cancel.
        /// Passing null disables the Cancel button.
        /// </summary>
        public void SetCancelAction(Action? action)
        {
            onCancelPressed = action;
            CanCancel = action != null;
        }

        /// <summary>Invoked by the Cancel button.</summary>
        [RelayCommand]
        private void Cancel() => onCancelPressed?.Invoke();
    }
}
