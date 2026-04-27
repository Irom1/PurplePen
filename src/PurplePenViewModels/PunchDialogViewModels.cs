// PunchDialogViewModels.cs
//
// ViewModels for PunchcardLayoutDialog and PunchPatternDialog.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PurplePen;

namespace PurplePen.ViewModels
{
    // ─────────────────────────────────────────────────────────────────────────
    // PunchcardLayoutDialogViewModel
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ViewModel for the Punch Card Layout dialog.
    /// Stores direction (LR/RL, TB/BT) and grid dimensions (rows × columns).
    /// </summary>
    public partial class PunchcardLayoutDialogViewModel : ViewModelBase
    {
        // Encode direction as a single index so radio buttons stay mutually exclusive.
        // 0 = LR/BT (default), 1 = LR/TB, 2 = RL/BT, 3 = RL/TB
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLRBT))]
        [NotifyPropertyChangedFor(nameof(IsLRTB))]
        [NotifyPropertyChangedFor(nameof(IsRLBT))]
        [NotifyPropertyChangedFor(nameof(IsRLTB))]
        private int orderIndex;

        [ObservableProperty] private decimal boxesAcross = PunchcardAppearance.defaultBoxesAcross;
        [ObservableProperty] private decimal boxesDown = PunchcardAppearance.defaultBoxesDown;

        // ── Radio button bool properties ──────────────────────────────────────

        /// <summary>Left-to-right, bottom-to-top.</summary>
        public bool IsLRBT { get => OrderIndex == 0; set { if (value) OrderIndex = 0; } }

        /// <summary>Left-to-right, top-to-bottom.</summary>
        public bool IsLRTB { get => OrderIndex == 1; set { if (value) OrderIndex = 1; } }

        /// <summary>Right-to-left, bottom-to-top.</summary>
        public bool IsRLBT { get => OrderIndex == 2; set { if (value) OrderIndex = 2; } }

        /// <summary>Right-to-left, top-to-bottom.</summary>
        public bool IsRLTB { get => OrderIndex == 3; set { if (value) OrderIndex = 3; } }

        public PunchcardLayoutDialogViewModel() { }

        /// <summary>Populates from an existing <see cref="PunchcardFormat"/>.</summary>
        public void Initialize(PunchcardFormat format)
        {
            BoxesAcross = format.boxesAcross;
            BoxesDown = format.boxesDown;

            OrderIndex = (format.leftToRight, format.topToBottom) switch {
                (true, false) => 0,
                (true, true) => 1,
                (false, false) => 2,
                _ => 3,
            };
        }

        /// <summary>Returns a <see cref="PunchcardFormat"/> from the current state.</summary>
        public PunchcardFormat GetPunchcardFormat()
        {
            PunchcardFormat fmt = new PunchcardFormat();
            fmt.boxesAcross = (int)BoxesAcross;
            fmt.boxesDown = (int)BoxesDown;
            (fmt.leftToRight, fmt.topToBottom) = OrderIndex switch {
                0 => (true, false),
                1 => (true, true),
                2 => (false, false),
                _ => (false, true),
            };
            return fmt;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PunchPatternDialogViewModel
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A single toggleable dot in the punch-pattern grid.
    /// </summary>
    public partial class DotViewModel : ViewModelBase
    {
        /// <summary>Row index in the grid (0-based).</summary>
        public int Row { get; }

        /// <summary>Column index in the grid (0-based).</summary>
        public int Col { get; }

        [ObservableProperty] private bool isChecked;

        public DotViewModel(int row, int col) { Row = row; Col = col; }
    }

    /// <summary>
    /// A control code entry in the PunchPatternDialog code list.
    /// </summary>
    public partial class CodeItemViewModel : ViewModelBase
    {
        /// <summary>The control code string.</summary>
        public string Code { get; }

        /// <summary>True when a non-empty pattern has been defined for this code.</summary>
        [ObservableProperty] private bool hasPattern;

        public CodeItemViewModel(string code, bool hasPattern)
        {
            Code = code;
            HasPattern = hasPattern;
        }
    }

    /// <summary>
    /// ViewModel for the Punch Patterns dialog.
    /// Manages a code list, a 7×7 dot grid, and the punch card layout format.
    /// </summary>
    public partial class PunchPatternDialogViewModel : ViewModelBase
    {
        private static readonly int GridSize = PunchcardAppearance.gridSize;

        private Dictionary<string, PunchPattern> patternDictionary = new();
        private PunchcardFormat punchcardFormat = new();
        private bool suppressSelectionSync;

        /// <summary>Sorted list of control codes with pattern-defined flags.</summary>
        public ObservableCollection<CodeItemViewModel> Codes { get; } = new();

        /// <summary>Currently selected code item.</summary>
        [ObservableProperty] private CodeItemViewModel? selectedCode;

        /// <summary>Flat 7×7 dot grid, row-major (row 0 col 0 … row 6 col 6).</summary>
        public ObservableCollection<DotViewModel> Dots { get; } = new();

        public PunchPatternDialogViewModel()
        {
            // Populate the fixed 7×7 dot grid.
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    Dots.Add(new DotViewModel(r, c));

            // Track dot changes so the HasPattern flag on the selected item stays current.
            foreach (DotViewModel dot in Dots)
                dot.PropertyChanged += (_, _) => UpdateSelectedHasPattern();
        }

        /// <summary>Populates from the controller data.</summary>
        public void Initialize(Dictionary<string, PunchPattern> patterns, PunchcardFormat format)
        {
            patternDictionary = new Dictionary<string, PunchPattern>(patterns);
            punchcardFormat = (PunchcardFormat)format.Clone();

            List<string> codes = new List<string>(patternDictionary.Keys);
            codes.Sort(Util.CompareCodes);

            Codes.Clear();
            foreach (string code in codes)
                Codes.Add(new CodeItemViewModel(code, patternDictionary[code] != null));

            SelectedCode = Codes.Count > 0 ? Codes[0] : null;
        }

        /// <summary>Returns the updated pattern dictionary after the user edits.</summary>
        public Dictionary<string, PunchPattern> GetAllPunchPatterns()
        {
            SaveCurrentPattern();
            return new Dictionary<string, PunchPattern>(patternDictionary);
        }

        /// <summary>Returns the punch card format (possibly updated via the sub-dialog).</summary>
        public PunchcardFormat GetPunchcardFormat() => (PunchcardFormat)punchcardFormat.Clone();

        partial void OnSelectedCodeChanged(CodeItemViewModel? oldValue, CodeItemViewModel? newValue)
        {
            if (suppressSelectionSync) return;

            // Save dots for the previously selected code.
            if (oldValue != null)
                patternDictionary[oldValue.Code] = DotsToPattern();

            // Load dots for the newly selected code.
            if (newValue != null)
                PatternToDots(patternDictionary.TryGetValue(newValue.Code, out PunchPattern? p) ? p : null);
        }

        // ── Punch Card Layout sub-dialog command ──────────────────────────────

        /// <summary>Opens the Punch Card Layout sub-dialog.</summary>
        [RelayCommand]
        private async Task ShowPunchcardLayout()
        {
            PunchcardLayoutDialogViewModel vm = new PunchcardLayoutDialogViewModel();
            vm.Initialize(punchcardFormat);
            if (await Services.DialogService.ShowDialogAsync(vm))
                punchcardFormat = vm.GetPunchcardFormat();
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private void SaveCurrentPattern()
        {
            if (SelectedCode != null)
                patternDictionary[SelectedCode.Code] = DotsToPattern();
        }

        private void PatternToDots(PunchPattern? punch)
        {
            suppressSelectionSync = true;
            try {
                foreach (DotViewModel dot in Dots)
                    dot.IsChecked = punch != null && punch.dots[dot.Row, dot.Col];
            }
            finally {
                suppressSelectionSync = false;
            }
        }

        private PunchPattern? DotsToPattern()
        {
            PunchPattern punch = new PunchPattern();
            punch.size = GridSize;
            punch.dots = new bool[GridSize, GridSize];
            foreach (DotViewModel dot in Dots)
                punch.dots[dot.Row, dot.Col] = dot.IsChecked;
            return punch.IsEmpty ? null : punch;
        }

        private void UpdateSelectedHasPattern()
        {
            if (SelectedCode != null)
                SelectedCode.HasPattern = Dots.Any(d => d.IsChecked);
        }
    }
}
