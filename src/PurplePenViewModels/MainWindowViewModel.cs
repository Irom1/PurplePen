// MainWindowViewModel.cs
//
// This is the "ViewModel" in the MVVM (Model-View-ViewModel) pattern.
// It holds the data and commands that the UI (the "View") binds to.
// It does NOT contain UI text or localized strings — those belong in the View
// (via resource files like UIText.resx referenced directly from XAML).
//
// We use CommunityToolkit.Mvvm source generators to eliminate boilerplate.
// The generators look at special attributes ([ObservableProperty], [RelayCommand])
// and auto-generate the repetitive code (property change notifications, ICommand
// implementations) at compile time. This keeps the code here minimal.
//
// IMPORTANT: The class must be "partial" so the source generators can add
// their generated code in a separate file behind the scenes.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        Controller? controller = null;
        SymbolDB symbolDB = null!;
        long changeNum = 0;         // When this changes, state information needs to be updated in the UI.
        bool updatingTabs = false;  // Guard to prevent re-entrant controller calls during UpdateTabs.


        [ObservableProperty]
        private IMapDisplay? mapDisplay;

        [ObservableProperty]
        private IMapViewerHighlight[]? mapHighlights;

        [ObservableProperty]
        private DescriptionViewerViewModel descriptionViewerViewModel = new DescriptionViewerViewModel();

        /// <summary>
        /// The names of the course tabs displayed in the tab strip.
        /// </summary>
        public ObservableCollection<string> TabNames { get; } = new();

        /// <summary>
        /// The index of the currently selected course tab.
        /// Setting this notifies the controller of the tab change.
        /// </summary>
        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private TextPart[] selectedObjectDescription = new TextPart[0];

        #region State change notifications.

        /// <summary>
        /// Called when the selected tab index changes.
        /// Notifies the controller so it can update the active course.
        /// </summary>
        partial void OnSelectedTabIndexChanged(int value)
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            if (!updatingTabs && value >= 0 && value < TabNames.Count) {
                controller.SelectTab(value);
            }
        }

        #endregion


        #region State updating on idle

        // This is called when the application becomes idle after processing input.
        // We can use this to update the UI in response to changes that may have occurred.
        public void UpdateStateOnIdle()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            UpdateMenusToolbarButtons();   // This needs updating even if other things haven't changed.
#if !PORTING
            UpdateStatusText();
#endif

            if (controller.HasStateChanged(ref changeNum)) {
                UpdateWindowTitle();
                UpdateMapFile();
                UpdateTabs();
                UpdateCourse();
                UpdateDescription();
                UpdateSelection();
                UpdateSelectionPanel();
                UpdateHighlight();
#if !PORTING
                UpdateTopology();
                UpdatePrintArea();
                UpdatePartBanner();
                UpdateTopologyHighlight();
                UpdateCustomSymbolText();
                CheckForMissingFonts();
                CheckForNonRenderableObjects(true, false);
#endif
            }

#if !PORTING
            if (checkForUpdatedMapFile) {
                checkForUpdatedMapFile = false;
                controller.CheckForChangedMapFile();
            }
#endif
        }

        // Update the window title with the current file name.
        private void UpdateWindowTitle()
        {
#if !PORTING
#endif
        }

        // Update the map file on Display.
        private void UpdateMapFile()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            if (MapDisplay != controller.MapDisplay) {
                // The mapDisplay object is new. This currently o`nly happens on startup.
                MapDisplay = controller.MapDisplay;
                controller.MapDisplay.MapIntensity = UserSettings.Current.MapIntensity;
                controller.MapDisplay.AntiAlias = UserSettings.Current.MapHighQuality;
                controller.ShowAllControls = UserSettings.Current.ViewAllControls;
            }

            if (controller.MapDisplay.MapType != controller.MapType || controller.MapDisplay.FileName != controller.MapFileName || (controller.MapType == MapType.Bitmap && controller.MapDisplay.Dpi != controller.MapDpi)) {
                // A new map file has been loaded, or the DPI has changed.
#if !PORTING
                mapViewer.ZoomFactor = 1.0F;   // used if the map bounds are empty, then this zoom factor is preserved.
                ShowRectangle(mapDisplay.MapBounds);

                // Reset the OCAD file creating settings dialog to default settings.
                ocadCreationSettingsPrevious = null;
                bitmapCreationSettingsPrevious = null;
#endif
            }

#if PORTING
            // Why is this logic in MainFrame/MainWindow instead of in the Controller?
            if (controller.MapDisplay.OcadOverprintEffect != controller.OcadOverprintEffect) {
                controller.MapDisplay.OcadOverprintEffect = controller.OcadOverprintEffect;
            }

            if (controller.MapDisplay.LowerPurpleMapLayer != controller.LowerPurpleMapLayer) {
                controller.MapDisplay.LowerPurpleMapLayer = controller.LowerPurpleMapLayer;
            }
#endif
        }

        // Update the tab strip to match the current set of courses.
        // Avoids unnecessary collection changes when the tab names haven't changed.
        private void UpdateTabs()
        {
            updatingTabs = true;
            try {
                UpdateTabsCore();
            }
            finally {
                updatingTabs = false;
            }
        }

        private void UpdateTabsCore()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            string[] tabNames = controller.GetTabNames();

            // Update or add tab names.
            for (int i = 0; i < tabNames.Length; i++) {
                if (i >= TabNames.Count) {
                    TabNames.Add(tabNames[i]);
                }
                else if (TabNames[i] != tabNames[i]) {
                    TabNames[i] = tabNames[i];
                }
            }

            // Remove any extra tabs.
            while (TabNames.Count > tabNames.Length) {
                TabNames.RemoveAt(TabNames.Count - 1);
            }

            // Sync the selected tab from the controller.
            SelectedTabIndex = controller.ActiveTab;
        }

        // Update the course in the map pane.
        void UpdateCourse()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            controller.MapDisplay.SetCourse(controller.GetCourseLayout());
        }


        // Update the description with data from the controller.
        void UpdateDescription()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            CourseView.CourseViewKind kind;
            DescriptionLine[] description;
            bool isCoursePart, hasCustomLength;

            description = controller.GetDescription(out kind, out isCoursePart, out hasCustomLength);

            DescriptionData descriptionData = new DescriptionData(
                Description: description,
                CourseKind: kind,
                ScoreColumn: controller.GetScoreColumn(),
                IsCoursePart: isCoursePart,
                HasCustomLength: hasCustomLength,
                LangId: controller.GetDescriptionLanguage()
            );

            DescriptionViewerViewModel.DescriptionData = descriptionData;
        }

        // Update the selected line.
        void UpdateSelection()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            int firstLine, lastLine;
            controller.GetHighlightedDescriptionLines(out firstLine, out lastLine);
            this.DescriptionViewerViewModel.Selection = new SelectedLines(firstLine, lastLine);
        }

        // Update the selection panel with a description of the selection.
        void UpdateSelectionPanel()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            this.SelectedObjectDescription = controller.GetSelectionDescription();
        }

        // Update the highlights
        void UpdateHighlight()
        {
            if (controller == null)
                return;   // happens in design mode, for example.

            this.MapHighlights = controller.GetHighlights(Pane.Map);
        }

        #endregion // State updating on idle.


        #region Mouse events

        public DragAction MapViewerLeftButtonDown(PointF location, float pixelSize)
        { return controller?.LeftButtonDown(Pane.Map, location, pixelSize) ?? DragAction.None; }

        public DragAction MapViewerRightButtonDown(PointF location, float pixelSize)
        { return controller?.RightButtonDown(Pane.Map, location, pixelSize) ?? DragAction.None; }

        public void MapViewerLeftButtonUp(PointF location, float pixelSize)
        { controller?.LeftButtonUp(Pane.Map, location, pixelSize); }

        public void MapViewerRightButtonUp(PointF location, float pixelSize)
        { controller?.RightButtonUp(Pane.Map, location, pixelSize); }

        public async Task MapViewerLeftButtonClick(PointF location, float pixelSize)
        { await controller?.LeftButtonClick(Pane.Map, location, pixelSize)!; }

        public async Task MapViewerRightButtonClick(PointF location, float pixelSize)
        { await controller?.RightButtonClick(Pane.Map, location, pixelSize)!; }

        public void MapViewerLeftButtonDrag(PointF location, PointF locationStart, float pixelSize)
        { controller?.LeftButtonDrag(Pane.Map, location, locationStart, pixelSize); }

        public void MapViewerRightButtonDrag(PointF location, PointF locationStart, float pixelSize)
        { controller?.RightButtonDrag(Pane.Map, location, locationStart, pixelSize); }

        public async Task MapViewerLeftButtonEndDrag(PointF location, PointF locationStart, float pixelSize)
        { await controller?.LeftButtonEndDrag(Pane.Map, location, locationStart, pixelSize)!; }

        public async Task MapViewerRightButtonEndDrag(PointF location, PointF locationStart, float pixelSize)
        { await controller?.RightButtonEndDrag(Pane.Map, location, locationStart, pixelSize)!; }
        public void MapViewerLeftButtonCancelDrag()
        { controller?.LeftButtonCancelDrag(Pane.Map); }

        public void MapViewerRightButtonCancelDrag()
        { controller?.RightButtonCancelDrag(Pane.Map); }

        #endregion

    }
}
