// NewEventWizardPageViewModels.cs
//
// One ViewModel per wizard page for the New Event Wizard.
// Each implements INewEventPageViewModel so the master wizard ViewModel
// can ask the current page whether it can proceed before enabling Next.
//
// Migrated from WinForms PurplePen/NewEvent*.cs pages.

using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Implemented by every wizard page ViewModel. The master wizard checks
    /// CanProceed before enabling the Next / Finish button.
    /// </summary>
    public interface INewEventPageViewModel
    {
        bool CanProceed { get; }
        string PageTitle { get; }
    }

    // ---------------------------------------------------------------------------
    // Page 1 – Event Title
    // ---------------------------------------------------------------------------

    /// <summary>Event name entered by the user.</summary>
    public partial class NewEventTitlePageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string eventTitle = "";

        public bool CanProceed => !string.IsNullOrWhiteSpace(EventTitle);
        public string PageTitle => "Event Title";

        /// <summary>
        /// Returns the safe filename derived from the event title (invalid path
        /// characters removed, ".ppen" appended).
        /// </summary>
        public string GetEventFileName()
        {
            string safe = EventTitle;
            foreach (char c in Path.GetInvalidFileNameChars())
                safe = safe.Replace(c, '_');
            return safe.Trim() + ".ppen";
        }
    }

    // ---------------------------------------------------------------------------
    // Page 2 – Map File
    // ---------------------------------------------------------------------------

    /// <summary>Map file selection and validation.</summary>
    public partial class NewEventMapFilePageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        [NotifyPropertyChangedFor(nameof(HasError))]
        [NotifyPropertyChangedFor(nameof(HasInfo))]
        private string mapFileName = "";

        [ObservableProperty]
        private string errorMessage = "";

        [ObservableProperty]
        private string infoMessage = "";

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool HasInfo => !string.IsNullOrEmpty(InfoMessage) && !HasError;

        // Validated map metadata — populated when a valid file is chosen.
        public MapType MapType { get; private set; }
        public float MapScale { get; private set; }
        public float Dpi { get; private set; }
        public Size BitmapSize { get; private set; }
        public RectangleF MapBounds { get; private set; }
        public int? LowerPurpleMapLayer { get; private set; }

        public bool CanProceed => !string.IsNullOrEmpty(MapFileName) && !HasError;
        public string PageTitle => "Map File";

        /// <summary>
        /// Validates the chosen map file and updates all metadata properties.
        /// Call this after the user picks a file.
        /// </summary>
        public void ValidateAndSetMapFile(string path)
        {
            MapFileName = path;
            ErrorMessage = "";
            InfoMessage = "";

            if (string.IsNullOrEmpty(path))
                return;

            string errorText;
            float scale, dpi;
            Size bitmapSize;
            RectangleF mapBounds;
            MapType mapType;
            int? lowerPurple;

            if (CoreMapUtil.ValidateMapFile(path, out scale, out dpi, out bitmapSize,
                    out mapBounds, out mapType, out lowerPurple, out errorText)) {
                MapScale = scale;
                Dpi = dpi;
                BitmapSize = bitmapSize;
                MapBounds = mapBounds;
                MapType = mapType;
                LowerPurpleMapLayer = lowerPurple;
                InfoMessage = Path.GetFileName(path);
            }
            else {
                ErrorMessage = errorText;
            }

            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(HasInfo));
            OnPropertyChanged(nameof(CanProceed));
        }
    }

    // ---------------------------------------------------------------------------
    // Page 3 – Bitmap / PDF Scale (conditional — only for Bitmap and PDF maps)
    // ---------------------------------------------------------------------------

    /// <summary>DPI and map scale for bitmap/PDF map files.</summary>
    public partial class NewEventBitmapScalePageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string dpiText = "200";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string scaleText = "15000";

        /// <summary>True for PDF maps (DPI field is hidden; scale is all that matters).</summary>
        [ObservableProperty]
        private bool isPdf = false;

        public bool CanProceed
        {
            get {
                if (!IsPdf && (!float.TryParse(DpiText, out float d) || d <= 0))
                    return false;
                return float.TryParse(ScaleText, out float s) && s > 0;
            }
        }

        public string PageTitle => "Map Scale";

        public float Dpi => float.TryParse(DpiText, out float d) ? d : 200f;
        public float Scale => float.TryParse(ScaleText, out float s) ? s : 15000f;
    }

    // ---------------------------------------------------------------------------
    // Page 4 – Print Scale
    // ---------------------------------------------------------------------------

    /// <summary>Default print scale for All Controls.</summary>
    public partial class NewEventPrintScalePageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string printScaleText = "15000";

        public ObservableCollection<string> AvailablePrintScales { get; } = new();

        public bool CanProceed => float.TryParse(PrintScaleText, out float s) && s > 0;
        public string PageTitle => "Print Scale";

        public float PrintScale => float.TryParse(PrintScaleText, out float s) ? s : 15000f;

        /// <summary>Populate print scale list from map scale.</summary>
        public void InitializeScales(float mapScale)
        {
            AvailablePrintScales.Clear();
            foreach (float scale in MapUtil.PrintScaleList(mapScale))
                AvailablePrintScales.Add(scale.ToString());
            if (AvailablePrintScales.Count > 0 && string.IsNullOrEmpty(PrintScaleText))
                PrintScaleText = AvailablePrintScales[0];
        }
    }

    // ---------------------------------------------------------------------------
    // Page 5 – Paper Size
    // ---------------------------------------------------------------------------

    /// <summary>Page dimensions and margins.</summary>
    public partial class NewEventPaperSizePageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PageWidthHundreths))]
        [NotifyPropertyChangedFor(nameof(PageHeightHundreths))]
        private int selectedPaperSizeIndex = 0;

        [ObservableProperty]
        private bool landscape = false;

        [ObservableProperty]
        private int marginHundreths = 50; // 0.5 inch default

        public ObservableCollection<string> PaperSizeNames { get; } = new();

        // The underlying PrintingPaperSize objects — parallel to PaperSizeNames.
        private PrintingPaperSize[] paperSizes = Array.Empty<PrintingPaperSize>();

        public bool CanProceed => true;
        public string PageTitle => "Paper Size";

        public NewEventPaperSizePageViewModel()
        {
            paperSizes = PrintingStandards.StandardPaperSizes;
            foreach (PrintingPaperSize ps in paperSizes)
                PaperSizeNames.Add(Util.GetPaperSizeText(ps));
        }

        public int PageWidthHundreths
        {
            get {
                if (SelectedPaperSizeIndex < 0 || SelectedPaperSizeIndex >= paperSizes.Length)
                    return 827;
                PrintingPaperSize ps = paperSizes[SelectedPaperSizeIndex];
                bool effectiveLandscape = Landscape != ps.Landscape;
                float w = effectiveLandscape ? ps.SizeInHundreths.Height : ps.SizeInHundreths.Width;
                return (int)Math.Round(w);
            }
        }

        public int PageHeightHundreths
        {
            get {
                if (SelectedPaperSizeIndex < 0 || SelectedPaperSizeIndex >= paperSizes.Length)
                    return 1169;
                PrintingPaperSize ps = paperSizes[SelectedPaperSizeIndex];
                bool effectiveLandscape = Landscape != ps.Landscape;
                float h = effectiveLandscape ? ps.SizeInHundreths.Width : ps.SizeInHundreths.Height;
                return (int)Math.Round(h);
            }
        }

        /// <summary>Sets the best-fit paper size for the given map bounds and scale ratio.</summary>
        public void AutoSelectPageSize(RectangleF mapBounds, float printScaleRatio, MapType mapType)
        {
            int pageWidth, pageHeight, pageMargin;
            bool isLandscape;

            if (!mapBounds.IsEmpty && (mapType == MapType.PDF || mapType == MapType.Bitmap)) {
                MapUtil.GetExactPageSize(mapBounds, printScaleRatio, out pageWidth, out pageHeight, out isLandscape);
                pageMargin = 0;
            }
            else {
                CoreMapUtil.GetDefaultPageSize(mapBounds, printScaleRatio, out pageWidth, out pageHeight, out pageMargin, out isLandscape);
            }

            // Find the closest matching standard paper size.
            int bestIndex = 0;
            for (int i = 0; i < paperSizes.Length; i++) {
                int w = (int)Math.Round(paperSizes[i].SizeInHundreths.Width);
                int h = (int)Math.Round(paperSizes[i].SizeInHundreths.Height);
                if ((w == pageWidth && h == pageHeight) ||
                    (w == pageHeight && h == pageWidth)) {
                    bestIndex = i;
                    break;
                }
            }

            SelectedPaperSizeIndex = bestIndex;
            Landscape = isLandscape;
            MarginHundreths = pageMargin;
        }
    }

    // ---------------------------------------------------------------------------
    // Page 6 – Event File Directory
    // ---------------------------------------------------------------------------

    /// <summary>Directory where the .ppen file will be saved.</summary>
    public partial class NewEventDirectoryPageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectory))]
        private bool useMapDirectory = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string otherDirectory = "";

        public bool IsOtherDirectory => !UseMapDirectory;
        public bool CanProceed => UseMapDirectory || !string.IsNullOrWhiteSpace(OtherDirectory);
        public string PageTitle => "Event File Location";

        /// <summary>
        /// Returns the directory to save the event file in, given the map file path.
        /// </summary>
        public string GetEventDirectory(string mapFilePath)
        {
            if (UseMapDirectory)
                return Path.GetDirectoryName(mapFilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return OtherDirectory;
        }
    }

    // ---------------------------------------------------------------------------
    // Page 7 – IOF Standards
    // ---------------------------------------------------------------------------

    /// <summary>Map and description IOF standard selection.</summary>
    public partial class NewEventStandardsPageViewModel : ViewModelBase, INewEventPageViewModel
    {
        // Map standard: "2000", "2017", "Spr2019"
        [ObservableProperty] private bool mapStandard2000 = false;
        [ObservableProperty] private bool mapStandard2017 = true;
        [ObservableProperty] private bool mapStandardSpr2019 = false;

        // Description standard: "2004", "2018"
        [ObservableProperty] private bool descStandard2004 = false;
        [ObservableProperty] private bool descStandard2018 = true;

        public bool CanProceed => true;
        public string PageTitle => "Orienteering Standards";

        public NewEventStandardsPageViewModel()
        {
            // Restore user's previous choice.
            string mapStd = UserSettings.Current.NewEventMapStandard;
            MapStandard2000 = mapStd == "2000";
            MapStandard2017 = mapStd == "2017" || (!MapStandard2000 && mapStd != "Spr2019");
            MapStandardSpr2019 = mapStd == "Spr2019";

            string descStd = UserSettings.Current.NewEventDescriptionStandard;
            DescStandard2018 = descStd == "2018";
            DescStandard2004 = !DescStandard2018;
        }

        public string MapStandard
        {
            get {
                if (MapStandard2000) return "2000";
                if (MapStandardSpr2019) return "Spr2019";
                return "2017";
            }
        }

        public string DescriptionStandard => DescStandard2018 ? "2018" : "2004";
    }

    // ---------------------------------------------------------------------------
    // Page 8 – Control Numbering
    // ---------------------------------------------------------------------------

    /// <summary>Starting control code and invertible-code restriction.</summary>
    public partial class NewEventNumberingPageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty] private int firstCode = 31;
        [ObservableProperty] private bool disallowInvertibleCodes = false;

        public bool CanProceed => true;
        public string PageTitle => "Control Numbering";
    }

    // ---------------------------------------------------------------------------
    // Page 9 – Final confirmation
    // ---------------------------------------------------------------------------

    /// <summary>Shows the full event file path and any pre-creation errors.</summary>
    public partial class NewEventFinalPageViewModel : ViewModelBase, INewEventPageViewModel
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string eventFilePath = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        [NotifyPropertyChangedFor(nameof(CanProceed))]
        private string errorMessage = "";

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool CanProceed => !HasError && !string.IsNullOrEmpty(EventFilePath);
        public string PageTitle => "Create Event";
    }
}
