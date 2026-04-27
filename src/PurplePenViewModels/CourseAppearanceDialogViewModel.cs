// CourseAppearanceDialogViewModel.cs
//
// ViewModel for the Customize Course Appearance dialog.
// Holds all settings from CourseAppearance and converts between the raw
// ratio-based values stored in the model and the mm values shown in the UI.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for CourseAppearanceDialog. Initialised from a <see cref="CourseAppearance"/>
    /// and yields a new one via <see cref="GetCourseAppearance"/>.
    /// </summary>
    public partial class CourseAppearanceDialogViewModel : ViewModelBase
    {
        // Stored so we can convert mm ↔ ratios and reset standard sizes.
        private string mapStandard = "2000";
        private float defaultPurpleC, defaultPurpleM, defaultPurpleY, defaultPurpleK;
        private readonly List<int> mapLayerIds = new();

        // ── Item Sizes group ──────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CustomSizesOpacity))]
        [NotifyPropertyChangedFor(nameof(IsCustomSizesHitTestVisible))]
        private bool isStandardSizes;

        [ObservableProperty] private decimal controlCircleDiameter;
        [ObservableProperty] private decimal lineWidth;
        [ObservableProperty] private decimal centerDotDiameter;
        [ObservableProperty] private decimal numberHeight;
        [ObservableProperty] private int controlNumberStyleIndex;
        [ObservableProperty] private decimal outlineWidth;
        [ObservableProperty] private decimal legGapSize;
        [ObservableProperty] private int scaleItemSizesIndex;

        // ── Purple Color group ─────────────────────────────────────────────

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CustomPurpleOpacity))]
        [NotifyPropertyChangedFor(nameof(IsCustomPurpleHitTestVisible))]
        private bool isDefaultPurple;

        [ObservableProperty] private decimal purpleCyan;
        [ObservableProperty] private decimal purpleMagenta;
        [ObservableProperty] private decimal purpleYellow;
        [ObservableProperty] private decimal purpleBlack;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MapLayerPickerOpacity))]
        [NotifyPropertyChangedFor(nameof(IsMapLayerPickerHitTestVisible))]
        private int blendPurpleIndex;

        [ObservableProperty] private int selectedMapLayerIndex = -1;

        /// <summary>Blend purple mode options; has 3 items for OCAD maps, 2 otherwise.</summary>
        public ObservableCollection<string> BlendPurpleOptions { get; } = new();

        /// <summary>Map layer names for the "Above map layer" picker.</summary>
        public ObservableCollection<string> MapLayerNames { get; } = new();

        // ── Description Color group ────────────────────────────────────────

        [ObservableProperty] private int descriptionColorIndex;

        // ── Advanced group ─────────────────────────────────────────────────

        [ObservableProperty] private bool useOcadOverprint;
        [ObservableProperty] private bool usesOcadMap;

        // ── Opacity helpers ────────────────────────────────────────────────

        /// <summary>1.0 when custom sizes are active, 0.0 when using IOF standard sizes.</summary>
        public double CustomSizesOpacity => IsStandardSizes ? 0.0 : 1.0;

        /// <summary>False when using IOF standard sizes (blocks mouse interaction on hidden controls).</summary>
        public bool IsCustomSizesHitTestVisible => !IsStandardSizes;

        /// <summary>1.0 when a custom purple is active, 0.0 when using the map default.</summary>
        public double CustomPurpleOpacity => IsDefaultPurple ? 0.0 : 1.0;

        /// <summary>False when using the map default purple.</summary>
        public bool IsCustomPurpleHitTestVisible => !IsDefaultPurple;

        /// <summary>1.0 when the "Layer" blend mode is selected.</summary>
        public double MapLayerPickerOpacity => BlendPurpleIndex == 2 ? 1.0 : 0.0;

        /// <summary>True when the "Layer" blend mode is selected.</summary>
        public bool IsMapLayerPickerHitTestVisible => BlendPurpleIndex == 2;

        // ── Lifecycle ──────────────────────────────────────────────────────

        /// <summary>Parameterless constructor for the Avalonia designer.</summary>
        public CourseAppearanceDialogViewModel()
        {
            BlendPurpleOptions.Add("None");
            BlendPurpleOptions.Add("Blend");
            BlendPurpleOptions.Add("Layer");
            BlendPurpleIndex = 1;
            IsStandardSizes = true;
            IsDefaultPurple = true;
            ControlCircleDiameter = (decimal)NormalCourseAppearance.controlOutsideDiameter2000;
            LineWidth = (decimal)NormalCourseAppearance.lineThickness;
            NumberHeight = (decimal)NormalCourseAppearance.nominalControlNumberHeight;
        }

        /// <summary>
        /// Populates the ViewModel from a <see cref="CourseAppearance"/> and supplementary data.
        /// </summary>
        /// <param name="appearance">Current appearance settings.</param>
        /// <param name="usesOcadMap">Whether the underlying map is an OCAD/OOM file.</param>
        /// <param name="mapLayers">Ordered map layers for the layer picker (top→bottom).</param>
        /// <param name="defaultC">Default purple cyan component (0–1).</param>
        /// <param name="defaultM">Default purple magenta component (0–1).</param>
        /// <param name="defaultY">Default purple yellow component (0–1).</param>
        /// <param name="defaultK">Default purple black component (0–1).</param>
        public void Initialize(CourseAppearance appearance, bool usesOcadMap,
                               List<Pair<int, string>> mapLayers,
                               float defaultC, float defaultM, float defaultY, float defaultK)
        {
            mapStandard = appearance.mapStandard;
            defaultPurpleC = defaultC;
            defaultPurpleM = defaultM;
            defaultPurpleY = defaultY;
            defaultPurpleK = defaultK;
            UsesOcadMap = usesOcadMap;

            // Build the blend options list (only show "Layer" for OCAD maps).
            BlendPurpleOptions.Clear();
            BlendPurpleOptions.Add("None");
            BlendPurpleOptions.Add("Blend");
            if (usesOcadMap)
                BlendPurpleOptions.Add("Layer");

            // Map layers for the layer picker.
            MapLayerNames.Clear();
            mapLayerIds.Clear();
            foreach (Pair<int, string> layer in mapLayers) {
                mapLayerIds.Add(layer.First);
                MapLayerNames.Add(layer.Second);
            }

            // Item sizes.
            ControlCircleDiameter = (decimal)appearance.ControlCircleOutsideDiameter;
            LineWidth = (decimal)(NormalCourseAppearance.lineThickness * appearance.lineWidth);
            CenterDotDiameter = (decimal)appearance.centerDotDiameter;
            NumberHeight = (decimal)(NormalCourseAppearance.nominalControlNumberHeight * appearance.numberHeight);

            ControlNumberStyleIndex = (!appearance.numberBold && !appearance.numberRoboto) ? 0
                                    : (appearance.numberBold && !appearance.numberRoboto) ? 1
                                    : (!appearance.numberBold && appearance.numberRoboto) ? 2
                                    : 3;

            OutlineWidth = (decimal)appearance.numberOutlineWidth;
            LegGapSize = (decimal)appearance.autoLegGapSize;
            ScaleItemSizesIndex = appearance.itemScaling == ItemScaling.None ? 0
                                : appearance.itemScaling == ItemScaling.RelativeToMap ? 1
                                : 2;

            IsStandardSizes = appearance.controlCircleSize == 1.0F
                           && appearance.lineWidth == 1.0F
                           && appearance.numberHeight == 1.0F
                           && appearance.centerDotDiameter == 0.0F;

            // Purple color.
            PurpleCyan = (decimal)(appearance.purpleC * 100F);
            PurpleMagenta = (decimal)(appearance.purpleM * 100F);
            PurpleYellow = (decimal)(appearance.purpleY * 100F);
            PurpleBlack = (decimal)(appearance.purpleK * 100F);
            IsDefaultPurple = appearance.useDefaultPurple;

            int blendIndex = appearance.purpleColorBlend == PurpleColorBlend.None ? 0
                           : appearance.purpleColorBlend == PurpleColorBlend.Blend ? 1
                           : 2;
            // Clamp: if !usesOcadMap the "Layer" option (index 2) doesn't exist.
            BlendPurpleIndex = (blendIndex >= BlendPurpleOptions.Count) ? 1 : blendIndex;

            int layerIndex = mapLayerIds.IndexOf(appearance.mapLayerForLowerPurple);
            SelectedMapLayerIndex = layerIndex;

            // Description color + advanced.
            DescriptionColorIndex = appearance.descriptionsPurple ? 1 : 0;
            UseOcadOverprint = appearance.useOcadOverprint;
        }

        /// <summary>Builds a <see cref="CourseAppearance"/> from the current ViewModel state.</summary>
        public CourseAppearance GetCourseAppearance()
        {
            CourseAppearance result = new CourseAppearance();
            result.mapStandard = mapStandard;

            float baseDiameter = mapStandard == "2017" ? NormalCourseAppearance.controlOutsideDiameter2017
                               : mapStandard == "Spr2019" ? NormalCourseAppearance.controlOutsideDiameterSpr2019
                               : NormalCourseAppearance.controlOutsideDiameter2000;

            if (IsStandardSizes) {
                result.controlCircleSize = 1.0F;
                result.lineWidth = 1.0F;
                result.numberHeight = 1.0F;
                result.centerDotDiameter = 0.0F;
            }
            else {
                result.controlCircleSize = (float)ControlCircleDiameter / baseDiameter;
                result.lineWidth = (float)LineWidth / NormalCourseAppearance.lineThickness;
                result.centerDotDiameter = (float)CenterDotDiameter;
                result.numberHeight = (float)NumberHeight / NormalCourseAppearance.nominalControlNumberHeight;
            }

            result.numberBold = ControlNumberStyleIndex == 1 || ControlNumberStyleIndex == 3;
            result.numberRoboto = ControlNumberStyleIndex == 2 || ControlNumberStyleIndex == 3;
            result.numberOutlineWidth = (float)OutlineWidth;
            result.autoLegGapSize = (float)LegGapSize;
            result.itemScaling = ScaleItemSizesIndex == 0 ? ItemScaling.None
                               : ScaleItemSizesIndex == 1 ? ItemScaling.RelativeToMap
                               : ItemScaling.RelativeTo15000;

            result.purpleColorBlend = BlendPurpleIndex == 0 ? PurpleColorBlend.None
                                    : BlendPurpleIndex == 1 ? PurpleColorBlend.Blend
                                    : PurpleColorBlend.UpperLowerPurple;

            result.mapLayerForLowerPurple = (SelectedMapLayerIndex >= 0 && SelectedMapLayerIndex < mapLayerIds.Count)
                ? mapLayerIds[SelectedMapLayerIndex]
                : -1;

            result.useDefaultPurple = IsDefaultPurple;
            result.purpleC = (float)(PurpleCyan / 100);
            result.purpleM = (float)(PurpleMagenta / 100);
            result.purpleY = (float)(PurpleYellow / 100);
            result.purpleK = (float)(PurpleBlack / 100);

            result.descriptionsPurple = DescriptionColorIndex == 1;
            result.useOcadOverprint = UseOcadOverprint;

            return result;
        }

        // ── WinForms-style reset helpers called from code-behind ──────────

        /// <summary>
        /// Resets size fields to IOF standard values. Called when "Use IOF standard sizes" is checked.
        /// </summary>
        public void ResetToStandardSizes()
        {
            float baseDiameter = mapStandard == "2017" ? NormalCourseAppearance.controlOutsideDiameter2017
                               : mapStandard == "Spr2019" ? NormalCourseAppearance.controlOutsideDiameterSpr2019
                               : NormalCourseAppearance.controlOutsideDiameter2000;
            ControlCircleDiameter = (decimal)baseDiameter;
            LineWidth = (decimal)NormalCourseAppearance.lineThickness;
            NumberHeight = (decimal)NormalCourseAppearance.nominalControlNumberHeight;
            CenterDotDiameter = (decimal)NormalCourseAppearance.centerDotDiameter;
        }

        /// <summary>
        /// Resets purple CMYK fields to the map default. Called when "Use purple color from map" is checked.
        /// </summary>
        public void ResetToDefaultPurple()
        {
            PurpleCyan = (decimal)(defaultPurpleC * 100F);
            PurpleMagenta = (decimal)(defaultPurpleM * 100F);
            PurpleYellow = (decimal)(defaultPurpleY * 100F);
            PurpleBlack = (decimal)(defaultPurpleK * 100F);
        }
    }
}
