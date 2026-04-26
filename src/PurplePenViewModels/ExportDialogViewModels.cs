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
