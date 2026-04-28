// MoveControlChoiceDialogViewModel.cs
//
// ViewModel for the MoveControlChoiceDialog.
// Presented when the user drags a control that is shared between multiple courses,
// prompting whether to move just this course's instance or all courses.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Move Control Choice dialog. Holds the control code and
    /// course list for display, and captures the user's choice after the dialog closes.
    /// </summary>
    public partial class MoveControlChoiceDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// The control code being moved. Used to format the explanation and duplicate-button text.
        /// </summary>
        [ObservableProperty]
        private string controlCode = "";

        /// <summary>
        /// Newline-delimited list of other courses that share this control.
        /// </summary>
        [ObservableProperty]
        private string otherCourses = "";

        /// <summary>
        /// The user's choice: Yes = move in all courses, No = create new control, Cancel = do nothing.
        /// Defaults to Cancel (safe default if the user closes the window without choosing).
        /// Set by the View before closing.
        /// </summary>
        public YesNoCancel ChosenResult { get; set; } = YesNoCancel.Cancel;
    }
}
