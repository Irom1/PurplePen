// MainWindow.axaml.cs
//
// Code-behind for the main window. Handles UI events that need
// direct window interaction (like showing modal dialogs), which
// don't fit cleanly into the ViewModel layer.

using Avalonia.Controls;
using Avalonia.Interactivity;
using AvUtil;
using PurplePen;
using PurplePen.ViewModels;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AvPurplePen.Views
{
    /// <summary>
    /// The main application window.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes the main window and its components.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            ApplicationIdleService.ApplicationIdle += ApplicationIdle;
            descriptionViewer.Change += DescriptionViewer_Change;
        }

        // Event fired when the user changes something in the description control.
        private void DescriptionViewer_Change(object sender, DescriptionChangeKind kind, int line, int box, object newValue)
        {
#if !PORTING
            //TODO: Implemented this.
#endif
        }

        // Mouse activity in the main map viewer.
        private void MapViewer_MouseActivity(object? sender, PanAndZoom.MouseEventArgs e)
        {
            MainWindowViewModel? vm = this.DataContext as MainWindowViewModel;
            if (vm == null)
                return;

            // Only left and right buttons have meaning.
            if (e.Button != PanAndZoom.MouseButton.LeftButton && e.Button != PanAndZoom.MouseButton.RightButton)
                return;

            bool isRightButton = (e.Button == PanAndZoom.MouseButton.RightButton);
            PointF location = Conv.ToPointF(e.WorldLocation);
            PointF locationStart = Conv.ToPointF(e.WorldDragStart);
            float pixelSize = mapViewer.PixelSize;
            DragAction dragAction = DragAction.None;
            
            switch (e.Action) {
            case PanAndZoom.MouseAction.Down:
                if (isRightButton)
                    dragAction = vm.MapViewerRightButtonDown(location, pixelSize);
                else
                    dragAction = vm.MapViewerLeftButtonDown(location, pixelSize);
                break;

            case PanAndZoom.MouseAction.Move:
                // nothing to do on pure mouse move; status bar is updated by idle.
                break;

            case PanAndZoom.MouseAction.Drag:
                if (isRightButton)
                    vm.MapViewerRightButtonDrag(location, locationStart, pixelSize);
                else
                    vm.MapViewerLeftButtonDrag(location, locationStart, pixelSize);
                break;

            case PanAndZoom.MouseAction.Up:
                if (isRightButton) 
                    vm.MapViewerRightButtonUp(location, pixelSize);
                else
                    vm.MapViewerLeftButtonUp(location, pixelSize);
                break;

            case PanAndZoom.MouseAction.DragEnd:
                if (isRightButton)
                    vm.MapViewerRightButtonEndDrag(location, locationStart, pixelSize);
                else
                    vm.MapViewerLeftButtonEndDrag(location, locationStart, pixelSize);
                break;

            case PanAndZoom.MouseAction.Click:
                if (isRightButton)
                    vm.MapViewerRightButtonClick(location, pixelSize);
                else
                    vm.MapViewerLeftButtonClick(location, pixelSize);
                break;

            case PanAndZoom.MouseAction.DragCancel:
                if (isRightButton)
                    vm.MapViewerRightButtonCancelDrag();
                else
                    vm.MapViewerLeftButtonCancelDrag();
                break;

            case PanAndZoom.MouseAction.Hover:
#if !PORTING
                // handle hover
#endif
                break;

            default:
                break;
            }

            switch (dragAction) {
            case DragAction.None:
                e.MouseDownResult = PanAndZoom.MouseDownResult.None; break;
            case DragAction.SuppressClick:
                e.MouseDownResult = PanAndZoom.MouseDownResult.SuppressClick; break;
            case DragAction.MapDrag:
                e.MouseDownResult = PanAndZoom.MouseDownResult.BeginPanning;  break;
            case DragAction.ImmediateDrag:
                e.MouseDownResult = PanAndZoom.MouseDownResult.ImmediateDrag; break;
            case DragAction.DelayedDrag:
                e.MouseDownResult = PanAndZoom.MouseDownResult.DelayedDrag; break;
            default:
                break;
            }
        }


        // This is called when the application becomes idle after processing input. We can use this to update
        // the UI in response to changes that may have occurred.
        private void ApplicationIdle(object? sender, System.EventArgs e)
        {
            if (this.IsVisible) {
                // The application is idle. If the application state has changed, update the
                // user interface to match.
                if (this.DataContext is MainWindowViewModel viewModel) {
                    viewModel.UpdateStateOnIdle();
                }
            }
        }
    }
}
