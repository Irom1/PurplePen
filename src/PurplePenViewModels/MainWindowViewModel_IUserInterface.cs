// IUserInterface implementation part of MainWindowViewModel.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PurplePen.ViewModels
{
    public partial class MainWindowViewModel: IUserInterface
    {
        private ProgressDialogViewModel? progressDialogViewModel;
        public void Initialize(Controller controller, SymbolDB symbolDB)
        {
            this.controller = controller;
            this.symbolDB = symbolDB;

            DescriptionViewerViewModel.SymbolDB = symbolDB;
            DescriptionViewerViewModel.Controller = controller;
            CoursePartBannerViewModel.Controller = controller;
        }

        public Size Size => new Size(1024, 768);

        public void QueueIdleEvent()
        {
            Services.ServiceProvider.GetRequiredService<IApplicationIdleService>().QueueIdleEvent();
        }

        public void PostDelayedAction(Action action)
        {
            Services.ServiceProvider.GetRequiredService<IPostMessage>().PostMessage(action);
        }

        public async Task InfoMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Information
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task WarningMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Warning
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task ErrorMessage(string message)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Error
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        public async Task<bool> OKCancelMessage(string message, bool okDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.OkCancel,
                DefaultButton = okDefault ? MessageBoxButton.Ok : MessageBoxButton.Cancel,
                Icon = MessageBoxIcon.Information
            };

            await Services.DialogService.ShowDialogAsync(vm);
            return vm.ChosenButton == MessageBoxButton.Ok;
        }

        public async Task<YesNoCancel> YesNoCancelQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.YesNoCancel,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };

            await Services.DialogService.ShowDialogAsync(vm);

            if (vm.ChosenButton == MessageBoxButton.Yes)
                return YesNoCancel.Yes;
            else if (vm.ChosenButton == MessageBoxButton.No)
                return YesNoCancel.No;
            else
                return YesNoCancel.Cancel;
        }

        public async Task<bool> YesNoQuestion(string message, bool yesDefault)
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = message,
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = yesDefault ? MessageBoxButton.Yes : MessageBoxButton.No,
                Icon = MessageBoxIcon.Question
            };

            await Services.DialogService.ShowDialogAsync(vm);
            return vm.ChosenButton == MessageBoxButton.Yes;
        }

        public async Task<YesNoCancel> MovingSharedControl(string controlCode, string otherCourses)
        {
            string message = string.Format("Control {0} is used in other courses: {1}\n\nYes = move shared control\nNo = create a new control\nCancel = do nothing.", controlCode, otherCourses);
            return await YesNoCancelQuestion(message, true);
        }

        public void ShowProgressDialog(bool knownDuration, Action onCancelPressed)
        {
            progressDialogViewModel = new ProgressDialogViewModel {
                IsIndeterminate = !knownDuration
            };
            progressDialogViewModel.SetCancelAction(onCancelPressed);
            Services.DialogService.ShowProgressWindow(progressDialogViewModel);
        }

        public bool UpdateProgressDialog(string info, double fractionDone)
        {
            if (progressDialogViewModel != null) {
                progressDialogViewModel.StatusText = info;
                progressDialogViewModel.FractionDone = fractionDone;
                progressDialogViewModel.IsIndeterminate = false;
            }
            return false;  // false = continue operation
        }

        public void EndProgressDialog()
        {
            Services.DialogService.CloseProgressWindow();
            progressDialogViewModel = null;
        }

        public string GetOpenFileName()
        {
            // Some legacy call paths still use this synchronous API.
            // For now, return null rather than throwing.
            return string.Empty;
        }

        public async Task<bool> FindMissingMapFile(string missingMapFile)
        {
            await WarningMessage(string.Format("Could not find map file: {0}", missingMapFile));
            return false;
        }

        public bool GetCurrentLocation(out PointF location, out float pixelSize)
        {
#if PORTING
            // TODO: get correct pixelSize.
            if (MouseLocationInMap.HasValue) {
                location = MouseLocationInMap.Value;
                pixelSize = 0.1F;
                return true;
            }
            else {
                location = new PointF();
                pixelSize = 0.1F;
                return false;
            }
#endif
        }

        public void InitiateMapDragging(PointF initialPos, PointerButton buttonEnd)
        {
            //throw new NotImplementedException();
        }

        public int LogicalToDeviceUnits(int value)
        {
            return value;
        }


        public void ShowTopologyView()
        {
            // Topology view is not ported in Avalonia yet.
        }

    }
}
