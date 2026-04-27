// EventDialogViewModels.cs
//
// ViewModels for Event and Edit menu dialogs:
//   AutoNumberingDialogViewModel    — Event/Auto Numbering
//   AddForkDialogViewModel          — Add/Add Variation
//   MapIssueChoiceDialogViewModel   — Add/Map Issue (three-way choice dialog)
//   AddTextLineDialogViewModel      — Add/Text Line
//   UnusedControlsDialogViewModel   — Event/Remove Unused Controls
//   ChangeMapFileDialogViewModel    — Event/Change Map File
//   ChangeAllCodesDialogViewModel   — Event/Change Codes

using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace PurplePen.ViewModels
{
    // -----------------------------------------------------------------------
    // Auto Numbering
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the Event / Auto Numbering dialog.
    /// Configures automatic control-code numbering.
    /// </summary>
    public partial class AutoNumberingDialogViewModel : ViewModelBase
    {
        private const int MinCode = 31;
        private const int MaxCode = 999;

        /// <summary>Starting code for newly created controls (31–999).</summary>
        [ObservableProperty]
        private int firstCode = 31;

        /// <summary>When true, codes readable upside-down (e.g. 68/89) are skipped.</summary>
        [ObservableProperty]
        private bool disallowInvertibleCodes;

        /// <summary>When true, existing controls are renumbered as well as new ones.</summary>
        [ObservableProperty]
        private bool renumberExisting;

        /// <summary>Design-time constructor.</summary>
        public AutoNumberingDialogViewModel() { }

        /// <summary>Runtime constructor — pre-populated from the controller.</summary>
        public AutoNumberingDialogViewModel(int firstCode, bool disallowInvertibleCodes)
        {
            FirstCode = Math.Max(MinCode, Math.Min(MaxCode, firstCode));
            DisallowInvertibleCodes = disallowInvertibleCodes;
            RenumberExisting = false;
        }
    }

    // -----------------------------------------------------------------------
    // Add Variation (Fork / Loop)
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the Add / Add Variation dialog.
    /// Lets the user choose Fork or Loop mode and the number of branches.
    /// </summary>
    public partial class AddForkDialogViewModel : ViewModelBase
    {
        /// <summary>True when Loop mode is selected; false for Fork.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsForkMode))]
        [NotifyPropertyChangedFor(nameof(IsLoopMode))]
        [NotifyPropertyChangedFor(nameof(ForkModeOpacity))]
        [NotifyPropertyChangedFor(nameof(LoopModeOpacity))]
        [NotifyPropertyChangedFor(nameof(SummaryText))]
        private bool loop;

        /// <summary>Number of branches or loops (2–10).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SummaryText))]
        private int numberOfBranches = 2;

        public bool IsForkMode => !Loop;
        public bool IsLoopMode => Loop;

        /// <summary>Opacity for controls only relevant in fork mode (1.0 or 0.0).</summary>
        public double ForkModeOpacity => IsForkMode ? 1.0 : 0.0;

        /// <summary>Opacity for controls only relevant in loop mode (1.0 or 0.0).</summary>
        public double LoopModeOpacity => IsLoopMode ? 1.0 : 0.0;

        /// <summary>Dynamic summary sentence shown below the branch-count combo.</summary>
        public string SummaryText
        {
            get
            {
                if (Loop) {
                    return string.Format(MiscText.LoopSummary,
                        NumberOfBranches + 1,
                        Util.Factorial(NumberOfBranches));
                }
                else {
                    List<int> participants = PossibleRelayParticipants(NumberOfBranches);
                    return string.Format(MiscText.ForkSummary,
                        string.Join(", ", participants.Select(x => x.ToString())));
                }
            }
        }

        /// <summary>Choices (2–10) for the branch/loop count ComboBox.</summary>
        public IReadOnlyList<int> BranchCountChoices { get; } =
            new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        /// <summary>Design-time constructor.</summary>
        public AddForkDialogViewModel() { }

        // Returns team sizes from 2..20 that evenly divide into numForks groups.
        private static List<int> PossibleRelayParticipants(int numForks)
        {
            List<int> result = new List<int>();
            for (int i = 2; i <= 20; ++i)
                if (i % numForks == 0)
                    result.Add(i);
            return result;
        }
    }

    // -----------------------------------------------------------------------
    // Map Issue Choice
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the Add / Map Issue choice dialog.
    /// The user clicks one of three buttons; the dialog closes immediately
    /// with the chosen <see cref="MapIssueKind"/>.
    /// </summary>
    public partial class MapIssueChoiceDialogViewModel : ViewModelBase
    {
        /// <summary>The kind chosen by the user. Valid only when the dialog returned true.</summary>
        public MapIssueKind SelectedKind { get; set; } = MapIssueKind.Beginning;

        /// <summary>Design-time constructor.</summary>
        public MapIssueChoiceDialogViewModel() { }
    }

    // -----------------------------------------------------------------------
    // Add Text Line
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the Add / Text Line dialog.
    /// Lets the user type freeform text and choose where it appears in the course description.
    /// </summary>
    public partial class AddTextLineDialogViewModel : ViewModelBase
    {
        /// <summary>Text entered by the user (newline-separated; use GetTextLine() for pipe-delimited result).</summary>
        [ObservableProperty]
        private string textLineText = "";

        /// <summary>0 = Above [object], 1 = Below [object].</summary>
        [ObservableProperty]
        private int positionIndex;

        /// <summary>0 = This course only, 1 = All courses with [object].</summary>
        [ObservableProperty]
        private int coursesIndex;

        /// <summary>False when only "all courses" applies; disables the Courses ComboBox.</summary>
        [ObservableProperty]
        private bool coursesEnabled = true;

        /// <summary>Localised + formatted position choices for the ComboBox.</summary>
        public ObservableCollection<string> PositionItems { get; } = new();

        /// <summary>Localised + formatted courses choices for the ComboBox.</summary>
        public ObservableCollection<string> CoursesItems { get; } = new();

        /// <summary>Design-time constructor — uses placeholder text for combo items.</summary>
        public AddTextLineDialogViewModel()
        {
            PositionItems.Add(string.Format(MiscText.AddTextLine_Above, "[object]"));
            PositionItems.Add(string.Format(MiscText.AddTextLine_Below, "[object]"));
            CoursesItems.Add(MiscText.AddTextLine_ThisCourse);
            CoursesItems.Add(string.Format(MiscText.AddTextLine_AllCourses, "[object]"));
        }

        /// <summary>
        /// Runtime constructor — formats combo items with the control or leg name
        /// returned by <c>controller.CanAddTextLine</c>.
        /// </summary>
        public AddTextLineDialogViewModel(string objectName, bool enableThisCourse)
        {
            PositionItems.Add(string.Format(MiscText.AddTextLine_Above, objectName));
            PositionItems.Add(string.Format(MiscText.AddTextLine_Below, objectName));
            CoursesItems.Add(MiscText.AddTextLine_ThisCourse);
            CoursesItems.Add(string.Format(MiscText.AddTextLine_AllCourses, objectName));

            if (!enableThisCourse) {
                CoursesIndex = 1;
                CoursesEnabled = false;
            }
        }

        /// <summary>
        /// The TextLineKind encoded by the current combo selections.
        /// The setter updates the combo indices to match.
        /// </summary>
        public DescriptionLine.TextLineKind TextLineKind
        {
            get
            {
                if (PositionIndex == 0)
                    return CoursesIndex == 0
                        ? DescriptionLine.TextLineKind.BeforeCourseControl
                        : DescriptionLine.TextLineKind.BeforeControl;
                else
                    return CoursesIndex == 0
                        ? DescriptionLine.TextLineKind.AfterCourseControl
                        : DescriptionLine.TextLineKind.AfterControl;
            }
            set
            {
                PositionIndex = (value == DescriptionLine.TextLineKind.BeforeCourseControl ||
                                 value == DescriptionLine.TextLineKind.BeforeControl) ? 0 : 1;
                CoursesIndex  = (value == DescriptionLine.TextLineKind.BeforeCourseControl ||
                                 value == DescriptionLine.TextLineKind.AfterCourseControl) ? 0 : 1;
            }
        }

        /// <summary>
        /// Returns the text in pipe-delimited format (as expected by the controller),
        /// or null if the text box is empty.
        /// </summary>
        public string? GetTextLine()
        {
            string trimmed = TextLineText.Trim('\r', '\n');
            if (string.IsNullOrEmpty(trimmed))
                return null;
            return trimmed.Replace("\r\n", "|").Replace("\n", "|").Replace("\r", "|");
        }

        /// <summary>Populates TextLineText from a pipe-delimited string.</summary>
        public void SetTextLine(string? value)
        {
            TextLineText = value == null ? "" : value.Replace("|", "\n");
        }
    }

    // -----------------------------------------------------------------------
    // Unused Controls
    // -----------------------------------------------------------------------

    /// <summary>One row in the UnusedControlsDialog checked list.</summary>
    public partial class UnusedControlItem : ObservableObject
    {
        public Id<ControlPoint> Id { get; }
        public string Name { get; }

        [ObservableProperty]
        private bool isChecked = true;

        public UnusedControlItem(Id<ControlPoint> id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// ViewModel for the Event / Remove Unused Controls dialog.
    /// Presents a checked list of unused controls; the user deselects any they
    /// want to keep before clicking Delete.
    /// </summary>
    public partial class UnusedControlsDialogViewModel : ViewModelBase
    {
        /// <summary>All unused controls found in the event.</summary>
        public ObservableCollection<UnusedControlItem> Items { get; } = new();

        public UnusedControlsDialogViewModel() { }

        /// <summary>Populates the list and pre-checks every item.</summary>
        public void SetControlsToDelete(List<KeyValuePair<Id<ControlPoint>, string>> controls)
        {
            Items.Clear();
            foreach (KeyValuePair<Id<ControlPoint>, string> pair in controls)
                Items.Add(new UnusedControlItem(pair.Key, pair.Value));
        }

        /// <summary>Returns the Ids of every checked item.</summary>
        public List<Id<ControlPoint>> GetControlsToDelete()
        {
            return Items.Where(i => i.IsChecked).Select(i => i.Id).ToList();
        }
    }

    // -----------------------------------------------------------------------
    // Change Map File
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the Event / Change Map File dialog.
    /// Validates the chosen map file and exposes Scale/Dpi fields when needed.
    /// </summary>
    public partial class ChangeMapFileDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        [NotifyPropertyChangedFor(nameof(ShowScaleDpi))]
        [NotifyPropertyChangedFor(nameof(ShowDpi))]
        [NotifyPropertyChangedFor(nameof(ShowDpiOpacity))]
        [NotifyPropertyChangedFor(nameof(ShowError))]
        private string mapFile = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private string mapScaleText = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOkEnabled))]
        private string dpiText = "";

        [ObservableProperty]
        private string errorMessage = "";

        private MapType mapType = MapType.None;

        public MapType MapType => mapType;

        public float MapScale => float.TryParse(MapScaleText, out float s) ? s : 0f;
        public float Dpi => float.TryParse(DpiText, out float d) ? d : 0f;

        /// <summary>True when the Scale (and possibly DPI) panel should be visible.</summary>
        public bool ShowScaleDpi => mapType == MapType.Bitmap || mapType == MapType.PDF;

        /// <summary>True when the DPI row within the panel should be visible.</summary>
        public bool ShowDpi => mapType == MapType.Bitmap;

        /// <summary>Opacity for the DPI row controls (1.0 = visible, 0.0 = hidden but keeps layout).</summary>
        public double ShowDpiOpacity => mapType == MapType.Bitmap ? 1.0 : 0.0;

        /// <summary>True when the error message panel should be visible.</summary>
        public bool ShowError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>True when the OK button should be enabled.</summary>
        public bool IsOkEnabled
        {
            get
            {
                if (mapType == MapType.OCAD)
                    return true;
                if (mapType == MapType.Bitmap)
                    return float.TryParse(MapScaleText, out _) && float.TryParse(DpiText, out _);
                if (mapType == MapType.PDF)
                    return float.TryParse(MapScaleText, out _);
                return false;
            }
        }

        public ChangeMapFileDialogViewModel() { }

        partial void OnMapFileChanged(string value)
        {
            UpdateFromFile(value);
        }

        partial void OnMapScaleTextChanged(string value) { OnPropertyChanged(nameof(IsOkEnabled)); }
        partial void OnDpiTextChanged(string value) { OnPropertyChanged(nameof(IsOkEnabled)); }

        private void UpdateFromFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                mapType = MapType.None;
                ErrorMessage = "";
                MapScaleText = "";
                DpiText = "";
                return;
            }

            bool ok = CoreMapUtil.ValidateMapFile(
                path,
                out float scale, out float dpi, out Size _, out RectangleF _,
                out mapType, out int? _, out string errorText);

            if (ok)
            {
                ErrorMessage = "";
                if (mapType == MapType.OCAD)
                {
                    MapScaleText = scale.ToString();
                    DpiText = "";
                }
                else if (mapType == MapType.Bitmap)
                {
                    MapScaleText = scale.ToString();
                    DpiText = dpi.ToString();
                }
                else if (mapType == MapType.PDF)
                {
                    MapScaleText = scale.ToString();
                    DpiText = "";
                }
            }
            else
            {
                mapType = MapType.None;
                ErrorMessage = errorText ?? "";
                MapScaleText = "";
                DpiText = "";
            }

            OnPropertyChanged(nameof(ShowScaleDpi));
            OnPropertyChanged(nameof(ShowDpi));
            OnPropertyChanged(nameof(ShowDpiOpacity));
            OnPropertyChanged(nameof(ShowError));
            OnPropertyChanged(nameof(MapType));
            OnPropertyChanged(nameof(IsOkEnabled));
        }
    }

    // -----------------------------------------------------------------------
    // Change All Codes
    // -----------------------------------------------------------------------

    /// <summary>One editable row in the ChangeAllCodesDialog grid.</summary>
    public partial class CodeEntry : ObservableObject
    {
        public object Key { get; }
        public string OldCode { get; }

        [ObservableProperty]
        private string newCode;

        public CodeEntry(object key, string code)
        {
            Key = key;
            OldCode = code;
            newCode = code;
        }
    }

    /// <summary>
    /// ViewModel for the Event / Change Control Codes dialog.
    /// Exposes a list of <see cref="CodeEntry"/> items that the user edits.
    /// Duplicate-code checking is done in the view's OK handler.
    /// </summary>
    public partial class ChangeAllCodesDialogViewModel : ViewModelBase
    {
        /// <summary>Rows displayed in the grid.</summary>
        public ObservableCollection<CodeEntry> Entries { get; } = new();

        private EventDB? eventDB;

        public ChangeAllCodesDialogViewModel() { }

        public void SetEventDB(EventDB db) => eventDB = db;

        /// <summary>Populates the grid from controller.GetAllControlCodes().</summary>
        public void SetCodes(KeyValuePair<object, string>[] codes)
        {
            Entries.Clear();
            foreach (KeyValuePair<object, string> pair in codes)
                Entries.Add(new CodeEntry(pair.Key, pair.Value));
        }

        /// <summary>Returns the edited codes in the same format as the input.</summary>
        public KeyValuePair<object, string>[] GetCodes()
        {
            return Entries.Select(e => new KeyValuePair<object, string>(e.Key, e.NewCode)).ToArray();
        }

        /// <summary>
        /// Validates a proposed new code. Returns null on success, or an error message.
        /// </summary>
        public string? ValidateCode(string code)
        {
            if (!QueryEvent.IsLegalControlCode(code, out string reason))
                return reason;
            return null;
        }

        /// <summary>
        /// Checks for duplicate new codes. Returns the duplicate code string, or null.
        /// </summary>
        public string? FindDuplicateCode()
        {
            Dictionary<string, bool> seen = new();
            foreach (CodeEntry entry in Entries)
            {
                if (seen.ContainsKey(entry.NewCode))
                    return entry.NewCode;
                seen[entry.NewCode] = true;
            }
            return null;
        }
    }

    // -----------------------------------------------------------------------
    // Shared course-check item (ViewAdditionalCourses + ChangeDisplayedCourses)
    // -----------------------------------------------------------------------

    /// <summary>One checkable course row used in course-selector dialogs.</summary>
    public partial class CourseCheckItem : ObservableObject
    {
        public CourseDesignator Designator { get; }
        public Id<Course> CourseId => Designator.CourseId;
        public string Name { get; }

        [ObservableProperty]
        private bool isChecked;

        public CourseCheckItem(CourseDesignator designator, string name, bool isChecked = false)
        {
            Designator = designator;
            Name = name;
            this.isChecked = isChecked;
        }
    }

    // -----------------------------------------------------------------------
    // View Additional Courses
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the View / Show Additional Courses dialog.
    /// Lists every course except the currently active one; the user checks the
    /// ones they want overlaid on the map.
    /// </summary>
    public partial class ViewAdditionalCoursesDialogViewModel : ViewModelBase
    {
        public string InstructionText { get; private set; } = "";
        public ObservableCollection<CourseCheckItem> Courses { get; } = new();

        public ViewAdditionalCoursesDialogViewModel() { }

        /// <summary>
        /// Populates the list, excluding <paramref name="currentCourseId"/>.
        /// </summary>
        public void Initialize(EventDB eventDB, string currentCourseName, Id<Course> currentCourseId,
                               List<Id<Course>>? displayedCourses)
        {
            InstructionText = string.Format(MiscText.ViewAdditionalCourses_Instructions, currentCourseName);
            Courses.Clear();
            foreach (Id<Course> id in eventDB.AllCourseIds)
            {
                if (id == currentCourseId) continue;
                bool isChecked = displayedCourses != null && displayedCourses.Contains(id);
                Courses.Add(new CourseCheckItem(new CourseDesignator(id), eventDB.GetCourse(id).name, isChecked));
            }
        }

        public List<Id<Course>> GetSelectedCourseIds() =>
            Courses.Where(c => c.IsChecked).Select(c => c.CourseId).ToList();
    }

    // -----------------------------------------------------------------------
    // Change Displayed Courses (for special objects)
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the Item / Change Displayed Courses dialog.
    /// The user chooses which courses a special object (line, rectangle, etc.) appears on.
    /// </summary>
    public partial class ChangeDisplayedCoursesDialogViewModel : ViewModelBase
    {
        public ObservableCollection<CourseCheckItem> Courses { get; } = new();

        public ChangeDisplayedCoursesDialogViewModel() { }

        /// <summary>
        /// Populates the list from the EventDB.
        /// </summary>
        public void Initialize(EventDB eventDB, bool showAllControls,
                               CourseDesignator[] currentDisplayedCourses)
        {
            Courses.Clear();
            if (showAllControls)
            {
                bool ac = currentDisplayedCourses.Any(d => d == CourseDesignator.AllControls);
                Courses.Add(new CourseCheckItem(CourseDesignator.AllControls, MiscText.AllControls, ac));
            }
            foreach (Id<Course> id in eventDB.AllCourseIds)
            {
                CourseDesignator des = new CourseDesignator(id);
                bool isChecked = currentDisplayedCourses.Any(d => d == des);
                Courses.Add(new CourseCheckItem(des, eventDB.GetCourse(id).name, isChecked));
            }
        }

        public CourseDesignator[] GetSelectedDesignators() =>
            Courses.Where(c => c.IsChecked).Select(c => c.Designator).ToArray();
    }

    // -----------------------------------------------------------------------
    // Line Properties (shared by AddLine, AddRectangle, AddEllipse,
    // ChangeLineAppearance)
    // -----------------------------------------------------------------------

    /// <summary>
    /// ViewModel for the line-properties dialog used by Add/Line, Add/Rectangle,
    /// Add/Ellipse, and Item/Change Line Appearance.
    /// </summary>
    public partial class LinePropertiesDialogViewModel : ViewModelBase
    {
        // Set by caller before showing the dialog.
        public string Title { get; set; } = "";
        public string Explanation { get; set; } = "";
        public bool ShowRadius { get; set; }
        public bool ShowLineKind { get; set; }

        public double ShowRadiusOpacity => ShowRadius ? 1.0 : 0.0;
        public double ShowLineKindOpacity => ShowLineKind ? 1.0 : 0.0;

        /// <summary>0=Purple, 1=Lower Purple, 2=Black</summary>
        [ObservableProperty]
        private int colorIndex;

        /// <summary>ComboBox index for LineKind (0=Single, 1=Double, 2=Dashed).</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowGapSize))]
        [NotifyPropertyChangedFor(nameof(ShowDashSize))]
        [NotifyPropertyChangedFor(nameof(ShowGapSizeOpacity))]
        [NotifyPropertyChangedFor(nameof(ShowDashSizeOpacity))]
        private int lineKindIndex;

        private LineKind lineKind = LineKind.Single;

        [ObservableProperty]
        private decimal lineWidth = 0.35m;

        [ObservableProperty]
        private decimal gapSize = 0.3m;

        [ObservableProperty]
        private decimal dashSize = 2.0m;

        [ObservableProperty]
        private decimal cornerRadius = 1.0m;

        public bool ShowGapSize => lineKind != LineKind.Single;
        public bool ShowDashSize => lineKind == LineKind.Dashed;
        public double ShowGapSizeOpacity => ShowGapSize ? 1.0 : 0.0;
        public double ShowDashSizeOpacity => ShowDashSize ? 1.0 : 0.0;

        public LinePropertiesDialogViewModel() { }

        partial void OnLineKindIndexChanged(int value)
        {
            lineKind = value switch { 1 => LineKind.Double, 2 => LineKind.Dashed, _ => LineKind.Single };
        }

        /// <summary>Gets/sets the <see cref="LineKind"/>; syncs with <see cref="LineKindIndex"/>.</summary>
        public LineKind LineKind
        {
            get => lineKind;
            set
            {
                lineKind = value;
                LineKindIndex = value switch { LineKind.Double => 1, LineKind.Dashed => 2, _ => 0 };
            }
        }

        /// <summary>Gets the currently selected <see cref="SpecialColor"/>.</summary>
        public SpecialColor Color
        {
            get
            {
                return ColorIndex switch {
                    1 => SpecialColor.LowerPurple,
                    2 => SpecialColor.Black,
                    _ => SpecialColor.UpperPurple,
                };
            }
            set
            {
                ColorIndex = value.Kind switch {
                    SpecialColor.ColorKind.LowerPurple => 1,
                    SpecialColor.ColorKind.Black => 2,
                    _ => 0,
                };
            }
        }
    }
}
