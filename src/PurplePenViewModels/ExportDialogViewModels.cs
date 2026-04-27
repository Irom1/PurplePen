// ExportDialogViewModels.cs
//
// ViewModels for the three export dialogs:
//   CreateCoursePdfDialog   — File / Create PDF Files
//   CreateOcadFilesDialog   — File / Create OCAD Files
//   CreateImageFilesDialog  — File / Create Image Files
//
// Each VM exposes a GetSettings() method that assembles the appropriate
// settings struct for the Controller after the user clicks Create.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen.MapModel;

namespace PurplePen.ViewModels
{
    // One selectable course entry in the course list.
    public partial class CourseSelectionItem : ObservableObject
    {
        [ObservableProperty] private bool isSelected;

        public string Name { get; }
        public Id<Course> CourseId { get; }

        public CourseSelectionItem(string name, Id<Course> courseId, bool isSelected = true)
        {
            Name = name;
            CourseId = courseId;
            IsSelected = isSelected;
        }
    }

    // Shared base: course list + output directory for all three export dialogs.
    public abstract partial class ExportDialogViewModelBase : ViewModelBase
    {
        public ObservableCollection<CourseSelectionItem> Courses { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectoryVisible))]
        private bool useMapDirectory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectoryVisible))]
        private bool useFileDirectory = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsOtherDirectoryVisible))]
        private bool useOtherDirectory;

        [ObservableProperty] private string otherDirectory = "";
        [ObservableProperty] private string filePrefix = "";
        [ObservableProperty] private bool allCoursesSelected = true;

        public bool IsOtherDirectoryVisible => UseOtherDirectory;

        // When "All Courses" toggled, sync individual check states.
        partial void OnAllCoursesSelectedChanged(bool value)
        {
            foreach (CourseSelectionItem item in Courses)
                item.IsSelected = value;
        }

        // Call from each subclass constructor to populate the course list.
        protected void InitializeFromSettings(
            Controller controller,
            Id<Course>[]? selectedIds,
            bool allCourses,
            bool mapDir,
            bool fileDir,
            string? outputDir,
            string? prefix)
        {
            EventDB eventDB = controller.GetEventDB();

            // First entry is "All Controls" (tab[0] name).
            string allControlsLabel = controller.GetTabNames().FirstOrDefault() ?? "All Controls";
            Courses.Add(new CourseSelectionItem(allControlsLabel, Id<Course>.None));

            foreach (Id<Course> id in QueryEvent.SortedCourseIds(eventDB, false))
                Courses.Add(new CourseSelectionItem(eventDB.GetCourse(id).name, id));

            AllCoursesSelected = allCourses;
            if (!allCourses && selectedIds != null)
            {
                HashSet<int> selectedSet = new HashSet<int>(selectedIds.Select(x => x.id));
                foreach (CourseSelectionItem item in Courses)
                    item.IsSelected = selectedSet.Contains(item.CourseId.id);
            }

            UseMapDirectory = mapDir;
            UseFileDirectory = fileDir && !mapDir;
            UseOtherDirectory = !mapDir && !fileDir;
            OtherDirectory = outputDir ?? "";
            FilePrefix = prefix ?? "";
        }

        // Returns the IDs of checked courses; used when AllCourses = false.
        protected Id<Course>[] GetSelectedCourseIds()
        {
            return Courses.Where(c => c.IsSelected).Select(c => c.CourseId).ToArray();
        }
    }

    // ViewModel for File / Create PDF Files.
    public partial class CreateCoursePdfDialogViewModel : ExportDialogViewModelBase
    {
        // 0 = Course+map, 1 = Course only (maps to DontPrintBaseMap = index==1)
        [ObservableProperty] private int printBaseMapIndex;
        // 0 = Crop to single page (CropLargePrintArea=true), 1 = Multiple pages
        [ObservableProperty] private int multiPageIndex;
        [ObservableProperty] private int colorModelIndex = 1;   // 0=RGB, 1=CMYK
        [ObservableProperty] private bool printMapExchangesOnOneMap;
        [ObservableProperty] private int fileCreationIndex = 1; // 0=Single, 1=PerCourse, 2=PerCoursePart

        // False when the source map is a PDF (disables multi-page toggle).
        public bool EnableCropToggle { get; }

        // Design-time / parameterless constructor.
        public CreateCoursePdfDialogViewModel()
        {
            Courses.Add(new CourseSelectionItem("All Controls", Id<Course>.None));
            Courses.Add(new CourseSelectionItem("Blue", new Id<Course>(1)));
            Courses.Add(new CourseSelectionItem("Red", new Id<Course>(2)));
            EnableCropToggle = true;
        }

        public CreateCoursePdfDialogViewModel(Controller controller, CoursePdfSettings settings, bool enableCropToggle)
        {
            EnableCropToggle = enableCropToggle;
            InitializeFromSettings(controller, settings.CourseIds, settings.AllCourses,
                settings.mapDirectory, settings.fileDirectory, settings.outputDirectory, settings.filePrefix);
            PrintBaseMapIndex = settings.DontPrintBaseMap ? 1 : 0;
            MultiPageIndex = settings.CropLargePrintArea ? 0 : 1;
            ColorModelIndex = settings.ColorModel == ColorModel.CMYK ? 1 : 0;
            PrintMapExchangesOnOneMap = settings.PrintMapExchangesOnOneMap;
            FileCreationIndex = (int)settings.FileCreation;
        }

        // Assembles CoursePdfSettings from current VM state.
        public CoursePdfSettings GetSettings()
        {
            return new CoursePdfSettings {
                CourseIds = GetSelectedCourseIds(),
                AllCourses = AllCoursesSelected,
                DontPrintBaseMap = PrintBaseMapIndex == 1,
                CropLargePrintArea = MultiPageIndex == 0,
                ColorModel = ColorModelIndex == 1 ? ColorModel.CMYK : ColorModel.RGB,
                PrintMapExchangesOnOneMap = PrintMapExchangesOnOneMap,
                FileCreation = (CoursePdfSettings.PdfFileCreation)FileCreationIndex,
                mapDirectory = UseMapDirectory,
                fileDirectory = UseFileDirectory,
                outputDirectory = OtherDirectory,
                filePrefix = FilePrefix,
                ShowProgressDialog = true,
            };
        }
    }

    // ViewModel for File / Create OCAD Files.
    public partial class CreateOcadFilesDialogViewModel : ExportDialogViewModelBase
    {
        private MapFileFormat[] _formatDescriptors = System.Array.Empty<MapFileFormat>();

        public ObservableCollection<string> FormatNames { get; } = new();

        [ObservableProperty] private int selectedFormatIndex;

        // Design-time / parameterless constructor.
        public CreateOcadFilesDialogViewModel()
        {
            Courses.Add(new CourseSelectionItem("All Controls", Id<Course>.None));
            Courses.Add(new CourseSelectionItem("Blue", new Id<Course>(1)));
            FormatNames.Add("OCAD 8");
            FormatNames.Add("OCAD 9");
            _formatDescriptors = new[] {
                new MapFileFormat(MapFileFormatKind.OCAD, 8),
                new MapFileFormat(MapFileFormatKind.OCAD, 9)
            };
        }

        public CreateOcadFilesDialogViewModel(Controller controller, OcadCreationSettings settings, MapFileFormatKind restrictTo)
        {
            InitializeFromSettings(controller, settings.CourseIds, settings.AllCourses,
                settings.mapDirectory, settings.fileDirectory, settings.outputDirectory, settings.filePrefix);
            BuildFormatList(restrictTo);
            SelectFormat(settings.fileFormat);
        }

        private void BuildFormatList(MapFileFormatKind restrictTo)
        {
            (string label, MapFileFormat fmt)[] allFormats = {
                (MiscText.OCAD + " 6",   new MapFileFormat(MapFileFormatKind.OCAD, 6)),
                (MiscText.OCAD + " 7",   new MapFileFormat(MapFileFormatKind.OCAD, 7)),
                (MiscText.OCAD + " 8",   new MapFileFormat(MapFileFormatKind.OCAD, 8)),
                (MiscText.OCAD + " 9",   new MapFileFormat(MapFileFormatKind.OCAD, 9)),
                (MiscText.OCAD + " 10",  new MapFileFormat(MapFileFormatKind.OCAD, 10)),
                (MiscText.OCAD + " 11",  new MapFileFormat(MapFileFormatKind.OCAD, 11)),
                (MiscText.OCAD + " 12",  new MapFileFormat(MapFileFormatKind.OCAD, 12)),
                (MiscText.OCAD + " 2018",new MapFileFormat(MapFileFormatKind.OCAD, 2018)),
                (MiscText.OpenOrienteeringMapper + " 0.7 (.omap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, 6)),
                (MiscText.OpenOrienteeringMapper + " 0.7 (.xmap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.XMap, 6)),
                (MiscText.OpenOrienteeringMapper + " 0.8 (.omap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, 7)),
                (MiscText.OpenOrienteeringMapper + " 0.8 (.xmap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.XMap, 7)),
                (MiscText.OpenOrienteeringMapper + " 0.9 (.omap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.OMap, 9)),
                (MiscText.OpenOrienteeringMapper + " 0.9 (.xmap)", new MapFileFormat(MapFileFormatKind.OpenMapper, OpenMapperSubKind.XMap, 9)),
            };

            List<(string, MapFileFormat)> filtered = allFormats
                .Where(f => restrictTo == MapFileFormatKind.None || restrictTo == f.fmt.kind)
                .ToList();

            _formatDescriptors = filtered.Select(f => f.Item2).ToArray();
            foreach ((string label, MapFileFormat _) in filtered)
                FormatNames.Add(label);

            SelectedFormatIndex = _formatDescriptors.Length > 0 ? 0 : -1;
        }

        private void SelectFormat(MapFileFormat fmt)
        {
            for (int i = 0; i < _formatDescriptors.Length; i++)
            {
                if (_formatDescriptors[i].Equals(fmt))
                {
                    SelectedFormatIndex = i;
                    return;
                }
            }
        }

        // Assembles OcadCreationSettings from current VM state.
        public OcadCreationSettings GetSettings()
        {
            MapFileFormat fmt = SelectedFormatIndex >= 0 && SelectedFormatIndex < _formatDescriptors.Length
                ? _formatDescriptors[SelectedFormatIndex]
                : new MapFileFormat(MapFileFormatKind.OCAD, 8);
            return new OcadCreationSettings {
                CourseIds = GetSelectedCourseIds(),
                AllCourses = AllCoursesSelected,
                fileFormat = fmt,
                mapDirectory = UseMapDirectory,
                fileDirectory = UseFileDirectory,
                outputDirectory = OtherDirectory,
                filePrefix = FilePrefix,
            };
        }
    }

    // ---------------------------------------------------------------
    // IOF XML / GPX / KML / RouteGadget / Description PDF / Punchcard PDF
    // ---------------------------------------------------------------

    /// <summary>Settings for File/Export XML — chooses IOF XML 2.0 or 3.0.</summary>
    public partial class CreateXmlDialogViewModel : ViewModelBase
    {
        /// <summary>0 = IOF XML 2.0, 1 = IOF XML 3.0 (default).</summary>
        [ObservableProperty] private int xmlVersionIndex = 1;

        /// <summary>The actual IOF XML version number (2 or 3).</summary>
        public int XmlVersion => XmlVersionIndex == 0 ? 2 : 3;
    }

    /// <summary>Settings for File/Export GPX — course checklist and waypoint code prefix.</summary>
    public partial class CreateGpxDialogViewModel : ViewModelBase
    {
        /// <summary>All courses in the event; the user checks the ones to export.</summary>
        public ObservableCollection<CourseCheckItem> Courses { get; } = new();

        /// <summary>Prefix added to each control code in the GPX waypoint names.</summary>
        [ObservableProperty] private string codePrefix = "";

        /// <summary>Populates <see cref="Courses"/> from the event, all checked by default.</summary>
        public void Initialize(EventDB eventDB)
        {
            Courses.Clear();
            foreach (Id<Course> id in eventDB.AllCourseIds)
                Courses.Add(new CourseCheckItem(new CourseDesignator(id), eventDB.GetCourse(id).name, true));
        }

        /// <summary>Builds a <see cref="GpxCreationSettings"/> from the current UI state.</summary>
        public GpxCreationSettings GetSettings()
        {
            bool allChecked = Courses.All(c => c.IsChecked);
            return new GpxCreationSettings {
                AllCourses = allChecked,
                CourseIds = allChecked
                    ? System.Array.Empty<Id<Course>>()
                    : Courses.Where(c => c.IsChecked).Select(c => c.CourseId).ToArray(),
                CodePrefix = CodePrefix,
            };
        }
    }

    /// <summary>Settings for File/Create KML Files — courses, output directory, prefix, and file mode.</summary>
    public partial class CreateKmlFilesDialogViewModel : ViewModelBase
    {
        /// <summary>All courses in the event; the user checks the ones to export.</summary>
        public ObservableCollection<CourseCheckItem> Courses { get; } = new();

        /// <summary>True when using the directory that contains the event file.</summary>
        [ObservableProperty] private bool fileDirectory = true;

        /// <summary>Custom output directory path (used when <see cref="FileDirectory"/> is false).</summary>
        [ObservableProperty] private string outputDirectory = "";

        /// <summary>Optional prefix prepended to each output file name.</summary>
        [ObservableProperty] private string filePrefix = "";

        /// <summary>True = all courses in a single KML file; false = one file per course.</summary>
        [ObservableProperty] private bool singleFile = false;

        /// <summary>Populates the view model from an <see cref="ExportKmlSettings"/> object.</summary>
        public void Initialize(EventDB eventDB, ExportKmlSettings settings)
        {
            Courses.Clear();
            foreach (Id<Course> id in eventDB.AllCourseIds) {
                bool isChecked = settings.AllCourses
                    || (settings.CourseIds != null && settings.CourseIds.Any(c => c == id));
                Courses.Add(new CourseCheckItem(new CourseDesignator(id), eventDB.GetCourse(id).name, isChecked));
            }
            FileDirectory = settings.fileDirectory;
            OutputDirectory = settings.outputDirectory ?? "";
            FilePrefix = settings.filePrefix ?? "";
            SingleFile = settings.FileCreation == ExportKmlSettings.KmlFileCreation.SingleFile;
        }

        /// <summary>Builds an <see cref="ExportKmlSettings"/> from the current UI state.</summary>
        public ExportKmlSettings GetSettings()
        {
            bool allChecked = Courses.All(c => c.IsChecked);
            return new ExportKmlSettings {
                AllCourses = allChecked,
                CourseIds = allChecked
                    ? System.Array.Empty<Id<Course>>()
                    : Courses.Where(c => c.IsChecked).Select(c => c.CourseId).ToArray(),
                fileDirectory = FileDirectory,
                mapDirectory = false,
                outputDirectory = OutputDirectory,
                filePrefix = FilePrefix,
                FileCreation = SingleFile
                    ? ExportKmlSettings.KmlFileCreation.SingleFile
                    : ExportKmlSettings.KmlFileCreation.FilePerCourse,
            };
        }
    }

    /// <summary>Settings for File/Create Route Gadget Files — output directory, base name, and XML version.</summary>
    public partial class CreateRouteGadgetFilesDialogViewModel : ViewModelBase
    {
        /// <summary>True when using the directory that contains the event file.</summary>
        [ObservableProperty] private bool fileDirectory = true;

        /// <summary>Custom output directory path (used when <see cref="FileDirectory"/> is false).</summary>
        [ObservableProperty] private string outputDirectory = "";

        /// <summary>Base file name for the generated .xml and .gif files (without extension).</summary>
        [ObservableProperty] private string fileBaseName = "";

        /// <summary>0 = IOF XML 2.0, 1 = IOF XML 3.0 (default).</summary>
        [ObservableProperty] private int xmlVersionIndex = 1;

        /// <summary>Populates the view model from a <see cref="RouteGadgetCreationSettings"/> object.</summary>
        public void Initialize(RouteGadgetCreationSettings settings)
        {
            FileDirectory = settings.fileDirectory;
            OutputDirectory = settings.outputDirectory ?? "";
            FileBaseName = settings.fileBaseName ?? "";
            XmlVersionIndex = settings.xmlVersion == 2 ? 0 : 1;
        }

        /// <summary>Builds a <see cref="RouteGadgetCreationSettings"/> from the current UI state.</summary>
        public RouteGadgetCreationSettings GetSettings()
        {
            return new RouteGadgetCreationSettings {
                fileDirectory = FileDirectory,
                mapDirectory = false,
                outputDirectory = OutputDirectory,
                fileBaseName = FileBaseName,
                xmlVersion = XmlVersionIndex == 0 ? 2 : 3,
            };
        }
    }

    /// <summary>Settings for File/Create Description PDF — courses, count kind, box size, desc kind, paper size.</summary>
    public partial class CreateDescriptionPdfDialogViewModel : ViewModelBase
    {
        // A4=827x1169, A3=1169x1654, Letter=850x1100 (hundredths-of-an-inch).
        private static readonly (string Name, float W, float H)[] s_descPaperSizes = {
            ("A4", 827, 1169), ("A3", 1169, 1654), ("Letter", 850, 1100),
        };

        /// <summary>All courses in the event; the user checks the ones to export.</summary>
        public ObservableCollection<CourseCheckItem> Courses { get; } = new();

        /// <summary>0 = OneDescription, 1 = OnePage, 2 = DescriptionCount.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CountOpacity))]
        [NotifyPropertyChangedFor(nameof(CountEnabled))]
        private int countKindIndex = 0;

        /// <summary>Number of copies (active only when <see cref="CountKindIndex"/> == 2).</summary>
        [ObservableProperty] private int count = 1;

        /// <summary>Height of each description box in millimetres.</summary>
        [ObservableProperty] private decimal boxSize = 6m;

        /// <summary>0 = UseCourseDefault, 1 = Symbols, 2 = Text, 3 = SymbolsAndText.</summary>
        [ObservableProperty] private int descKindIndex = 0;

        /// <summary>0 = A4, 1 = A3, 2 = Letter.</summary>
        [ObservableProperty] private int paperSizeIndex = 0;

        /// <summary>Opacity of the count NumericUpDown (1.0 when active, 0.0 when inactive).</summary>
        public double CountOpacity => CountKindIndex == 2 ? 1.0 : 0.0;

        /// <summary>Whether the count NumericUpDown accepts input.</summary>
        public bool CountEnabled => CountKindIndex == 2;

        /// <summary>Populates the view model from a <see cref="DescriptionPrintSettings"/> object.</summary>
        public void Initialize(EventDB eventDB, DescriptionPrintSettings settings)
        {
            Courses.Clear();
            foreach (Id<Course> id in eventDB.AllCourseIds) {
                bool isChecked = settings.AllCourses
                    || (settings.CourseIds != null && settings.CourseIds.Any(c => c == id));
                Courses.Add(new CourseCheckItem(new CourseDesignator(id), eventDB.GetCourse(id).name, isChecked));
            }
            CountKindIndex = settings.CountKind switch {
                CorePrintingCountKind.OnePage => 1,
                CorePrintingCountKind.DescriptionCount => 2,
                _ => 0,
            };
            Count = settings.Count;
            BoxSize = (decimal)settings.BoxSize;
            DescKindIndex = settings.UseCourseDefault ? 0 : settings.DescKind switch {
                DescriptionKind.Text => 2,
                DescriptionKind.SymbolsAndText => 3,
                _ => 1,
            };
            PaperSizeIndex = 0; // default A4
        }

        /// <summary>Builds a <see cref="DescriptionPrintSettings"/> from the current UI state.</summary>
        public DescriptionPrintSettings GetSettings()
        {
            bool allChecked = Courses.All(c => c.IsChecked);
            bool useCourseDefault = DescKindIndex == 0;
            DescriptionKind descKind = DescKindIndex switch {
                2 => DescriptionKind.Text,
                3 => DescriptionKind.SymbolsAndText,
                _ => DescriptionKind.Symbols,
            };
            return new DescriptionPrintSettings {
                AllCourses = allChecked,
                CourseIds = allChecked
                    ? System.Array.Empty<Id<Course>>()
                    : Courses.Where(c => c.IsChecked).Select(c => c.CourseId).ToArray(),
                CountKind = CountKindIndex switch {
                    1 => CorePrintingCountKind.OnePage,
                    2 => CorePrintingCountKind.DescriptionCount,
                    _ => CorePrintingCountKind.OneDescription,
                },
                Count = Count,
                BoxSize = (float)BoxSize,
                UseCourseDefault = useCourseDefault,
                DescKind = descKind,
            };
        }

        /// <summary>Returns the paper size and 0.5" margins for use when creating the PDF.</summary>
        public PrintingPaperSizeWithMargins GetPaperSizeWithMargins()
        {
            int idx = System.Math.Clamp(PaperSizeIndex, 0, s_descPaperSizes.Length - 1);
            PrintingPaperSize ps = new PrintingPaperSize(s_descPaperSizes[idx].Name, s_descPaperSizes[idx].W, s_descPaperSizes[idx].H);
            return new PrintingPaperSizeWithMargins(ps, new PrintingMarginSize(50));
        }
    }

    /// <summary>Settings for File/Create Punchcard PDF — courses, count, box size, paper size.</summary>
    public partial class CreatePunchcardPdfDialogViewModel : ViewModelBase
    {
        // A4=827x1169, A3=1169x1654, Letter=850x1100 (hundredths-of-an-inch).
        private static readonly (string Name, float W, float H)[] s_punchPaperSizes = {
            ("A4", 827, 1169), ("A3", 1169, 1654), ("Letter", 850, 1100),
        };

        /// <summary>All courses in the event; the user checks the ones to export.</summary>
        public ObservableCollection<CourseCheckItem> Courses { get; } = new();

        /// <summary>Number of copies of each punchcard to include in the PDF.</summary>
        [ObservableProperty] private int count = 1;

        /// <summary>Height of each punch box in millimetres.</summary>
        [ObservableProperty] private decimal boxSize = 18m;

        /// <summary>0 = A4, 1 = A3, 2 = Letter.</summary>
        [ObservableProperty] private int paperSizeIndex = 0;

        /// <summary>Populates the view model from a <see cref="CorePunchPrintSettings"/> object.</summary>
        public void Initialize(EventDB eventDB, CorePunchPrintSettings settings)
        {
            Courses.Clear();
            foreach (Id<Course> id in eventDB.AllCourseIds) {
                bool isChecked = settings.AllCourses
                    || (settings.CourseIds != null && settings.CourseIds.Any(c => c == id));
                Courses.Add(new CourseCheckItem(new CourseDesignator(id), eventDB.GetCourse(id).name, isChecked));
            }
            Count = settings.Count;
            BoxSize = (decimal)settings.BoxSize;
            PaperSizeIndex = 0; // default A4
        }

        /// <summary>Builds a <see cref="CorePunchPrintSettings"/> from the current UI state.</summary>
        public CorePunchPrintSettings GetSettings()
        {
            bool allChecked = Courses.All(c => c.IsChecked);
            return new CorePunchPrintSettings {
                AllCourses = allChecked,
                CourseIds = allChecked
                    ? System.Array.Empty<Id<Course>>()
                    : Courses.Where(c => c.IsChecked).Select(c => c.CourseId).ToArray(),
                Count = Count,
                BoxSize = (float)BoxSize,
            };
        }

        /// <summary>Returns the paper size and 0.5" margins for use when creating the PDF.</summary>
        public PrintingPaperSizeWithMargins GetPaperSizeWithMargins()
        {
            int idx = System.Math.Clamp(PaperSizeIndex, 0, s_punchPaperSizes.Length - 1);
            PrintingPaperSize ps = new PrintingPaperSize(s_punchPaperSizes[idx].Name, s_punchPaperSizes[idx].W, s_punchPaperSizes[idx].H);
            return new PrintingPaperSizeWithMargins(ps, new PrintingMarginSize(50));
        }
    }

    // ViewModel for File / Create Image Files.
    public partial class CreateImageFilesDialogViewModel : ExportDialogViewModelBase
    {
        [ObservableProperty] private int bitmapKindIndex;       // 0=PNG, 1=JPEG, 2=GIF
        [ObservableProperty] private string dpiText = "200";
        [ObservableProperty] private int colorModelIndex = 1;   // 0=RGB, 1=CMYK
        [ObservableProperty] private bool worldFileEnabled;
        [ObservableProperty] private int worldFileIndex;        // 0=No, 1=Yes
        // 0 = Course+map, 1 = Course only
        [ObservableProperty] private int printBaseMapIndex;

        // Design-time / parameterless constructor.
        public CreateImageFilesDialogViewModel()
        {
            Courses.Add(new CourseSelectionItem("All Controls", Id<Course>.None));
            Courses.Add(new CourseSelectionItem("Blue", new Id<Course>(1)));
            WorldFileEnabled = true;
        }

        public CreateImageFilesDialogViewModel(Controller controller, BitmapCreationSettings settings, bool worldFileEnabled)
        {
            InitializeFromSettings(controller, settings.CourseIds, settings.AllCourses,
                settings.mapDirectory, settings.fileDirectory, settings.outputDirectory, settings.filePrefix);
            BitmapKindIndex = settings.ExportedBitmapKind switch {
                BitmapCreationSettings.BitmapKind.Jpeg => 1,
                BitmapCreationSettings.BitmapKind.Gif  => 2,
                _                                      => 0,
            };
            DpiText = settings.Dpi > 0 ? ((int)settings.Dpi).ToString() : "200";
            ColorModelIndex = settings.ColorModel == ColorModel.CMYK ? 1 : 0;
            WorldFileEnabled = worldFileEnabled;
            WorldFileIndex = settings.WorldFile ? 1 : 0;
            PrintBaseMapIndex = settings.DontPrintBaseMap ? 1 : 0;
        }

        // Assembles BitmapCreationSettings from current VM state.
        public BitmapCreationSettings GetSettings()
        {
            BitmapCreationSettings.BitmapKind kind = BitmapKindIndex switch {
                1 => BitmapCreationSettings.BitmapKind.Jpeg,
                2 => BitmapCreationSettings.BitmapKind.Gif,
                _ => BitmapCreationSettings.BitmapKind.Png,
            };
            float dpi = float.TryParse(DpiText, out float d) && d > 0 ? d : 200f;
            return new BitmapCreationSettings {
                CourseIds = GetSelectedCourseIds(),
                AllCourses = AllCoursesSelected,
                ExportedBitmapKind = kind,
                Dpi = dpi,
                ColorModel = ColorModelIndex == 1 ? ColorModel.CMYK : ColorModel.RGB,
                WorldFile = WorldFileIndex == 1,
                DontPrintBaseMap = PrintBaseMapIndex == 1,
                mapDirectory = UseMapDirectory,
                fileDirectory = UseFileDirectory,
                outputDirectory = OtherDirectory,
                filePrefix = FilePrefix,
            };
        }
    }
}
