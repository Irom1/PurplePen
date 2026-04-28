// SetPrintAreaDialogViewModel.cs
//
// ViewModel for the Set Print Area dialog. Exposes paper size, margin,
// orientation, and print area mode options as bindable properties.
// Communicates with the controller to preview and commit the print area.
//
// Ported from WinForms PurplePen/SetPrintAreaDialog.cs.

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the Set Print Area dialog.
    /// The caller must provide a Controller and PrintAreaKind via the runtime constructor.
    /// </summary>
    public partial class SetPrintAreaDialogViewModel : ViewModelBase
    {
        private readonly Controller? _controller;
        private readonly PrintAreaKind _printAreaKind;
        private PrintArea _printArea;
        private readonly PrintingPaperSize[] _standardSizes;
        private bool _updateInProgress;

        // Index of the "User Defined" entry at the end of PaperSizeNames.
        private int _userDefinedIndex;

        // ── Paper size combo ──────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsUserDefinedSize))]
        private int selectedPaperSizeIndex = 0;

        // ── Width / Height / Margin in user units (mm or in) ─────────────

        [ObservableProperty]
        private decimal pageWidthValue = 0;

        [ObservableProperty]
        private decimal pageHeightValue = 0;

        [ObservableProperty]
        private decimal pageMarginsValue = 0;

        // ── Orientation ───────────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsPortrait))]
        private bool landscape = false;

        // ── Checkboxes ────────────────────────────────────────────────────

        [ObservableProperty]
        private bool autoPrintArea = true;

        [ObservableProperty]
        private bool restrictToPageSize = true;

        // ── Unit display ──────────────────────────────────────────────────

        /// <summary>Label string shown after width/height/margin fields ("mm" or "in").</summary>
        public string UnitLabel => Util.IsCurrentCultureMetric() ? "mm" : "in";

        /// <summary>Format string for numeric up-downs (e.g. "0.0" or "0.00").</summary>
        public string FormatString => Util.IsCurrentCultureMetric() ? "0.0" : "0.00";

        /// <summary>Step increment for numeric up-downs.</summary>
        public decimal Increment => Util.IsCurrentCultureMetric() ? 1.0m : 0.05m;

        /// <summary>Maximum dimension value for numeric up-downs.</summary>
        public decimal MaxDimension => Util.IsCurrentCultureMetric() ? 5000m : 200m;

        /// <summary>True when Portrait orientation is selected (inverse of Landscape).</summary>
        public bool IsPortrait
        {
            get => !Landscape;
            set => Landscape = !value;
        }

        // ── Computed ──────────────────────────────────────────────────────

        /// <summary>True when the last paper size entry ("User Defined") is selected.</summary>
        public bool IsUserDefinedSize => SelectedPaperSizeIndex == _userDefinedIndex;

        /// <summary>Items for the paper size combo box.</summary>
        public ObservableCollection<string> PaperSizeNames { get; } = new();

        // ── Design-time constructor ───────────────────────────────────────

        /// <summary>Parameterless constructor for the Avalonia designer.</summary>
        public SetPrintAreaDialogViewModel()
        {
            _standardSizes = PrintingStandards.StandardPaperSizes;
            _printArea = PrintArea.DefaultPrintArea;
            _controller = null;
            _printAreaKind = PrintAreaKind.OneCourse;
            InitPaperSizeNames();
            UpdateDialogControls();
        }

        // ── Runtime constructor ───────────────────────────────────────────

        /// <summary>
        /// Runtime constructor.
        /// Loads the current print area from the controller and begins the interactive mode.
        /// </summary>
        /// <param name="controller">Application controller.</param>
        /// <param name="printAreaKind">Which scope of print area is being edited.</param>
        public SetPrintAreaDialogViewModel(Controller controller, PrintAreaKind printAreaKind)
        {
            _controller = controller;
            _printAreaKind = printAreaKind;
            _standardSizes = PrintingStandards.StandardPaperSizes;
            _printArea = (PrintArea)controller.GetCurrentPrintArea(printAreaKind).Clone();

            InitPaperSizeNames();
            UpdateDialogControls();

            // Put the controller into rectangle-select mode so it draws the print area overlay.
            controller.BeginSetPrintArea(printAreaKind, new NoopDisposable());
            // Push current print area to controller for the initial preview.
            SendPrintAreaUpdate();
        }

        // ── Internal helpers ──────────────────────────────────────────────

        /// <summary>Populates PaperSizeNames from the standard sizes plus a "User Defined" entry.</summary>
        private void InitPaperSizeNames()
        {
            PaperSizeNames.Clear();
            foreach (PrintingPaperSize ps in _standardSizes)
                PaperSizeNames.Add(Util.GetPaperSizeText(ps));
            PaperSizeNames.Add(MiscText.UserDefined);
            _userDefinedIndex = PaperSizeNames.Count - 1;
        }

        /// <summary>Populates all dialog controls from the current _printArea model.</summary>
        private void UpdateDialogControls()
        {
            _updateInProgress = true;

            AutoPrintArea = _printArea.autoPrintArea;
            RestrictToPageSize = _printArea.restrictToPageSize;

            int pageW = _printArea.pageWidth;
            int pageH = _printArea.pageHeight;

            if (pageW > 0 && pageH > 0) {
                // Find a matching standard size (ignoring orientation — compare short vs long sides).
                int bestIndex = _userDefinedIndex;
                for (int i = 0; i < _standardSizes.Length; i++) {
                    int sw = (int)Math.Round(_standardSizes[i].SizeInHundreths.Width);
                    int sh = (int)Math.Round(_standardSizes[i].SizeInHundreths.Height);
                    if ((sw == pageW && sh == pageH) || (sw == pageH && sh == pageW)) {
                        bestIndex = i;
                        break;
                    }
                }
                SelectedPaperSizeIndex = bestIndex;
                Landscape = _printArea.pageLandscape;
                PageWidthValue = Util.GetDistanceValue(pageW);
                PageHeightValue = Util.GetDistanceValue(pageH);
            }

            PageMarginsValue = Util.GetDistanceValue(_printArea.pageMargins);

            _updateInProgress = false;
        }

        /// <summary>Reads control state back into _printArea.</summary>
        private void UpdatePrintArea()
        {
            _printArea.autoPrintArea = AutoPrintArea;
            _printArea.restrictToPageSize = RestrictToPageSize;
            _printArea.pageLandscape = Landscape;
            _printArea.pageMargins = Util.GetDistanceFromValue(PageMarginsValue);

            if (IsUserDefinedSize) {
                _printArea.pageWidth = Util.GetDistanceFromValue(PageWidthValue);
                _printArea.pageHeight = Util.GetDistanceFromValue(PageHeightValue);
            }
            else if (SelectedPaperSizeIndex >= 0 && SelectedPaperSizeIndex < _standardSizes.Length) {
                PrintingPaperSize ps = _standardSizes[SelectedPaperSizeIndex];
                // Swap width/height if landscape orientation differs from the canonical size.
                bool effectiveLandscape = Landscape != ps.Landscape;
                if (effectiveLandscape) {
                    _printArea.pageWidth = (int)Math.Round(ps.SizeInHundreths.Height);
                    _printArea.pageHeight = (int)Math.Round(ps.SizeInHundreths.Width);
                }
                else {
                    _printArea.pageWidth = (int)Math.Round(ps.SizeInHundreths.Width);
                    _printArea.pageHeight = (int)Math.Round(ps.SizeInHundreths.Height);
                }
            }

            if (_controller != null)
                _printArea.printAreaRectangle = _controller.SetPrintAreaCurrentRectangle();
        }

        /// <summary>Pushes current print area settings to the controller for live preview.</summary>
        private void SendPrintAreaUpdate()
        {
            UpdatePrintArea();
            _controller?.SetPrintAreaUpdate(_printAreaKind, _printArea);
        }

        // ── Property-change callbacks ─────────────────────────────────────

        partial void OnSelectedPaperSizeIndexChanged(int value)
        {
            if (_updateInProgress)
                return;

            // Sync the displayed width/height to the newly selected standard size.
            if (!IsUserDefinedSize && value >= 0 && value < _standardSizes.Length) {
                _updateInProgress = true;
                PrintingPaperSize ps = _standardSizes[value];
                bool effectiveLandscape = Landscape != ps.Landscape;
                PageWidthValue = Util.GetDistanceValue(effectiveLandscape
                    ? (int)Math.Round(ps.SizeInHundreths.Height)
                    : (int)Math.Round(ps.SizeInHundreths.Width));
                PageHeightValue = Util.GetDistanceValue(effectiveLandscape
                    ? (int)Math.Round(ps.SizeInHundreths.Width)
                    : (int)Math.Round(ps.SizeInHundreths.Height));
                _updateInProgress = false;
            }

            SendPrintAreaUpdate();
        }

        partial void OnLandscapeChanged(bool value)
        {
            if (_updateInProgress)
                return;

            // Swap displayed width/height when flipping orientation on a standard size.
            if (!IsUserDefinedSize && SelectedPaperSizeIndex >= 0 && SelectedPaperSizeIndex < _standardSizes.Length) {
                _updateInProgress = true;
                decimal tmp = PageWidthValue;
                PageWidthValue = PageHeightValue;
                PageHeightValue = tmp;
                _updateInProgress = false;
            }

            SendPrintAreaUpdate();
        }

        partial void OnAutoPrintAreaChanged(bool value)
        {
            if (_updateInProgress)
                return;

            if (!value && _controller != null) {
                // Switching from auto to manual: pre-fill with the auto-generated rectangle.
                UpdatePrintArea();
                _printArea.autoPrintArea = true;
                _printArea.printAreaRectangle = _controller.GetPrintAreaRectangle(_printAreaKind, _printArea);
                _printArea.autoPrintArea = false;
            }

            SendPrintAreaUpdate();
        }

        partial void OnRestrictToPageSizeChanged(bool value)
        {
            if (!_updateInProgress)
                SendPrintAreaUpdate();
        }

        partial void OnPageWidthValueChanged(decimal value)
        {
            if (!_updateInProgress)
                SendPrintAreaUpdate();
        }

        partial void OnPageHeightValueChanged(decimal value)
        {
            if (!_updateInProgress)
                SendPrintAreaUpdate();
        }

        partial void OnPageMarginsValueChanged(decimal value)
        {
            if (!_updateInProgress)
                SendPrintAreaUpdate();
        }

        // ── Dialog actions ────────────────────────────────────────────────

        /// <summary>
        /// Called by the code-behind when the user clicks Done.
        /// Commits the print area to the event database via the controller.
        /// </summary>
        public void OnOk()
        {
            UpdatePrintArea();
            if (_controller != null)
                _printArea.printAreaRectangle = _controller.SetPrintAreaCurrentRectangle();
            _controller?.EndSetPrintArea(_printAreaKind, _printArea);
        }

        /// <summary>
        /// Called by the code-behind when the user clicks Cancel.
        /// Reverts the controller to its default mode.
        /// </summary>
        public void OnCancel()
        {
            _controller?.CancelMode();
        }

        // ── Private helpers ───────────────────────────────────────────────

        /// <summary>IDisposable with a no-op Dispose, passed to BeginSetPrintArea.</summary>
        private sealed class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
