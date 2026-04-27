// CourseDialogViewModels.cs
//
// ViewModels for five Course menu dialogs:
//   AllControlsPropertiesDialogViewModel  — Course/Properties (All Controls branch)
//   CourseLoadDialogViewModel             — Course/Competitor Load
//   CourseOrderDialogViewModel            — Course/Course Order
//   LegAssignmentsDialogViewModel         — sub-dialog for Relay Team Variations
//   TeamVariationsDialogViewModel         — Course/Relay Team Variations

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PurplePen.ViewModels
{
    // -----------------------------------------------------------------------
    // All Controls Properties
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the All Controls Properties dialog.
    /// Lets the user change the print scale and description kind for the
    /// "All Controls" pseudo-course.
    /// </summary>
    public partial class AllControlsPropertiesDialogViewModel : ViewModelBase
    {
        /// <summary>Available print scales derived from the map scale.</summary>
        public ObservableCollection<string> AvailablePrintScales { get; } = new();

        /// <summary>Currently typed or selected scale (editable ComboBox).</summary>
        [ObservableProperty]
        private string printScaleText = "10000";

        /// <summary>0=Symbols, 1=Text, 2=Symbols and text.</summary>
        [ObservableProperty]
        private int descKindIndex;

        /// <summary>Design-time / parameterless constructor.</summary>
        public AllControlsPropertiesDialogViewModel()
        {
            AvailablePrintScales.Add("5000");
            AvailablePrintScales.Add("10000");
            AvailablePrintScales.Add("15000");
        }

        /// <summary>Runtime constructor — pre-populates from current event state.</summary>
        public AllControlsPropertiesDialogViewModel(float mapScale, float printScale, DescriptionKind descKind)
        {
            foreach (float s in MapUtil.PrintScaleList(mapScale))
                AvailablePrintScales.Add(s.ToString());
            PrintScaleText = printScale.ToString();
            DescKindIndex = (int)descKind;
        }

        /// <summary>Parsed print scale; 0 if text is not a valid number.</summary>
        public float PrintScale => float.TryParse(PrintScaleText, out float s) ? s : 0;

        /// <summary>Description kind corresponding to the selected index.</summary>
        public DescriptionKind DescKind => (DescriptionKind)DescKindIndex;
    }

    // -----------------------------------------------------------------------
    // Course Load
    // -----------------------------------------------------------------------

    /// <summary>
    /// One row in the Course Load grid — a course name plus an editable
    /// competitor load count.
    /// </summary>
    public partial class CourseLoadItem : ObservableObject
    {
        private Controller.CourseLoadInfo _info;

        /// <summary>Course name (read-only column).</summary>
        public string CourseName => _info.courseName;

        /// <summary>Editable load text. Empty string means "no load set" (stored as −1).</summary>
        [ObservableProperty]
        private string loadText = "";

        /// <param name="info">Struct from the controller; internal courseId is preserved via copy.</param>
        public CourseLoadItem(Controller.CourseLoadInfo info)
        {
            _info = info;
            LoadText = info.load > 0 ? info.load.ToString() : "";
        }

        /// <summary>
        /// Returns a copy of the original struct with the load updated to the
        /// value the user typed.  The internal courseId is preserved by the copy.
        /// </summary>
        public Controller.CourseLoadInfo ToUpdatedInfo()
        {
            Controller.CourseLoadInfo updated = _info;
            updated.load = int.TryParse(LoadText, out int v) && v > 0 ? v : -1;
            return updated;
        }
    }

    /// <summary>ViewModel for the Course / Competitor Load dialog.</summary>
    public partial class CourseLoadDialogViewModel : ViewModelBase
    {
        /// <summary>One row per course, bound to the DataGrid.</summary>
        public ObservableCollection<CourseLoadItem> CourseLoads { get; } = new();

        /// <summary>Design-time / parameterless constructor.</summary>
        public CourseLoadDialogViewModel()
        {
            CourseLoads.Add(new CourseLoadItem(new Controller.CourseLoadInfo { courseName = "Blue", load = 25 }));
            CourseLoads.Add(new CourseLoadItem(new Controller.CourseLoadInfo { courseName = "Red", load = -1 }));
        }

        /// <summary>Runtime constructor — loads all courses from the controller.</summary>
        public CourseLoadDialogViewModel(Controller controller)
        {
            foreach (Controller.CourseLoadInfo info in controller.GetAllCourseLoads())
                CourseLoads.Add(new CourseLoadItem(info));
        }

        /// <summary>
        /// Returns the updated array to pass to <c>controller.SetAllCourseLoads()</c>.
        /// </summary>
        public Controller.CourseLoadInfo[] GetCourseLoads() =>
            CourseLoads.Select(item => item.ToUpdatedInfo()).ToArray();
    }

    // -----------------------------------------------------------------------
    // Course Order
    // -----------------------------------------------------------------------

    /// <summary>One row in the Course Order list.</summary>
    public class CourseOrderItem
    {
        /// <summary>Course name shown in the list.</summary>
        public string CourseName { get; }

        /// <summary>Mutable info struct; courseId is internal but preserved by copy.</summary>
        internal Controller.CourseOrderInfo Info { get; set; }

        /// <param name="info">Struct from the controller.</param>
        public CourseOrderItem(Controller.CourseOrderInfo info)
        {
            CourseName = info.courseName;
            Info = info;
        }
    }

    /// <summary>ViewModel for the Course / Course Order dialog.</summary>
    public partial class CourseOrderDialogViewModel : ViewModelBase
    {
        /// <summary>Ordered list of courses, bound to the ListBox.</summary>
        public ObservableCollection<CourseOrderItem> Courses { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanMoveUp))]
        [NotifyPropertyChangedFor(nameof(CanMoveDown))]
        private int selectedIndex = -1;

        /// <summary>Move Up button is enabled only when an item above exists.</summary>
        public bool CanMoveUp => SelectedIndex > 0;

        /// <summary>Move Down button is enabled only when an item below exists.</summary>
        public bool CanMoveDown => SelectedIndex >= 0 && SelectedIndex < Courses.Count - 1;

        /// <summary>Moves the selected course one position up.</summary>
        [RelayCommand]
        private void MoveUp()
        {
            if (!CanMoveUp) return;
            Courses.Move(SelectedIndex, SelectedIndex - 1);
            SelectedIndex--;
        }

        /// <summary>Moves the selected course one position down.</summary>
        [RelayCommand]
        private void MoveDown()
        {
            if (!CanMoveDown) return;
            Courses.Move(SelectedIndex, SelectedIndex + 1);
            SelectedIndex++;
        }

        /// <summary>Design-time / parameterless constructor.</summary>
        public CourseOrderDialogViewModel()
        {
            Courses.Add(new CourseOrderItem(new Controller.CourseOrderInfo { courseName = "Blue", sortOrder = 1 }));
            Courses.Add(new CourseOrderItem(new Controller.CourseOrderInfo { courseName = "Red", sortOrder = 2 }));
            Courses.Add(new CourseOrderItem(new Controller.CourseOrderInfo { courseName = "Green", sortOrder = 3 }));
        }

        /// <summary>Runtime constructor — loads all courses from the controller.</summary>
        public CourseOrderDialogViewModel(Controller controller)
        {
            foreach (Controller.CourseOrderInfo info in controller.GetAllCourseOrders())
                Courses.Add(new CourseOrderItem(info));
        }

        /// <summary>
        /// Returns the updated array (with sequential sortOrder values) to pass to
        /// <c>controller.SetAllCourseOrders()</c>.
        /// </summary>
        public Controller.CourseOrderInfo[] GetCourseOrders()
        {
            return Courses.Select((item, i) => {
                Controller.CourseOrderInfo info = item.Info;
                info.sortOrder = i + 1;
                return info;
            }).ToArray();
        }
    }

    // -----------------------------------------------------------------------
    // Leg Assignments (sub-dialog of Relay Team Variations)
    // -----------------------------------------------------------------------

    /// <summary>One row in the Leg Assignments grid — a branch code and its assigned legs.</summary>
    public partial class LegAssignmentItem : ObservableObject
    {
        /// <summary>Single-character branch code (e.g. "A", "B").</summary>
        public string BranchCode { get; }

        /// <summary>
        /// Comma/space-separated list of leg numbers the user types (e.g. "1, 3").
        /// Empty means no fixed assignment.
        /// </summary>
        [ObservableProperty]
        private string legsText = "";

        /// <summary>True for every other branch group — used for alternating row background.</summary>
        public bool IsAlternateGroup { get; }

        /// <param name="code">Branch character.</param>
        /// <param name="isAlternate">True for odd-numbered branch groups.</param>
        public LegAssignmentItem(char code, bool isAlternate)
        {
            BranchCode = code.ToString();
            IsAlternateGroup = isAlternate;
        }
    }

    /// <summary>ViewModel for the Leg Assignments sub-dialog.</summary>
    public partial class LegAssignmentsDialogViewModel : ViewModelBase
    {
        /// <summary>One row per branch code, bound to the DataGrid.</summary>
        public ObservableCollection<LegAssignmentItem> Items { get; } = new();

        /// <summary>Design-time / parameterless constructor.</summary>
        public LegAssignmentsDialogViewModel()
        {
            Items.Add(new LegAssignmentItem('A', false));
            Items.Add(new LegAssignmentItem('B', false));
            Items.Add(new LegAssignmentItem('C', true));
        }

        /// <summary>
        /// Runtime constructor.
        /// </summary>
        /// <param name="codes">Branch codes grouped by fork, from controller.GetLegAssignmentCodes().</param>
        /// <param name="existing">Current fixed assignments to pre-populate.</param>
        public LegAssignmentsDialogViewModel(List<char[]> codes, FixedBranchAssignments existing)
        {
            bool alt = false;
            foreach (char[] group in codes) {
                foreach (char c in group) {
                    LegAssignmentItem item = new LegAssignmentItem(c, alt);
                    if (existing.BranchIsFixed(c)) {
                        item.LegsText = string.Join(", ",
                            existing.FixedLegsForBranch(c).Select(l => (l + 1).ToString()));
                    }
                    Items.Add(item);
                }
                alt = !alt;
            }
        }

        /// <summary>
        /// Parses the leg text fields and builds a <see cref="FixedBranchAssignments"/> instance
        /// to pass back to the controller.
        /// Leg numbers are 1-based in the UI but 0-based in the model.
        /// </summary>
        public FixedBranchAssignments GetFixedBranchAssignments()
        {
            FixedBranchAssignments result = new FixedBranchAssignments();
            foreach (LegAssignmentItem item in Items) {
                char code = item.BranchCode[0];
                foreach (string s in item.LegsText.Split(
                    new[] { ' ', ';', ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (int.TryParse(s, out int leg))
                        result.AddBranchAssignment(code, leg - 1);
                }
            }
            return result;
        }
    }

    // -----------------------------------------------------------------------
    // Relay Team Variations
    // -----------------------------------------------------------------------

    /// <summary>ViewModel for the Course / Relay Team Variations dialog.</summary>
    public partial class TeamVariationsDialogViewModel : ViewModelBase
    {
        [ObservableProperty] private int numberOfTeams;
        [ObservableProperty] private int numberOfLegs = 1;
        [ObservableProperty] private int firstTeamNumber = 1;
        [ObservableProperty] private bool hideVariationsOnMap;

        /// <summary>Status message shown below the Calculate button.</summary>
        [ObservableProperty] private string statusText = "";

        /// <summary>
        /// The current fixed branch-to-leg assignments.
        /// Set by the Assign Legs sub-dialog in the code-behind.
        /// </summary>
        public FixedBranchAssignments FixedBranchAssignments { get; set; } = new FixedBranchAssignments();

        /// <summary>Default export file name provided by the controller.</summary>
        public string DefaultExportFileName { get; set; } = "";

        /// <summary>
        /// Controller reference set by the command handler.
        /// Needed by the dialog code-behind for Calculate, Assign Legs, and Export.
        /// Controller lives in PurplePenCore so ViewModels may reference it.
        /// </summary>
        public Controller? Controller { get; set; }

        /// <summary>Assembles a RelaySettings from current field values.</summary>
        public RelaySettings RelaySettings => new RelaySettings(
            FirstTeamNumber, NumberOfTeams, NumberOfLegs, FixedBranchAssignments);

        /// <summary>Design-time / parameterless constructor.</summary>
        public TeamVariationsDialogViewModel() { }

        /// <summary>Runtime constructor — pre-populates from current event relay settings.</summary>
        public TeamVariationsDialogViewModel(RelaySettings settings, bool hideVariations)
        {
            NumberOfTeams = settings.relayTeams;
            NumberOfLegs = settings.relayLegs;
            FirstTeamNumber = settings.firstTeamNumber;
            HideVariationsOnMap = hideVariations;
            FixedBranchAssignments = settings.relayBranchAssignments ?? new FixedBranchAssignments();
        }
    }
}
