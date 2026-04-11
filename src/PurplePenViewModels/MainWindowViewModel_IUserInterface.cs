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
        public void Initialize(Controller controller, SymbolDB symbolDB)
        {
            this.controller = controller;
            this.symbolDB = symbolDB;

            DescriptionViewerViewModel.SymbolDB = symbolDB;
            DescriptionViewerViewModel.Controller = controller;
        }

        public Size Size => throw new NotImplementedException();

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
            throw new NotImplementedException();
        }

        public async Task<YesNoCancel> YesNoCancelQuestion(string message, bool yesDefault)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> YesNoQuestion(string message, bool yesDefault)
        {
            throw new NotImplementedException();
        }

        public async Task<YesNoCancel> MovingSharedControl(string controlCode, string otherCourses)
        {
            throw new NotImplementedException();
        }

        public void ShowProgressDialog(bool knownDuration, Action onCancelPressed)
        {
            throw new NotImplementedException();
        }

        public bool UpdateProgressDialog(string info, double fractionDone)
        {
            throw new NotImplementedException();
        }

        public void EndProgressDialog()
        {
            throw new NotImplementedException();
        }

        public string GetOpenFileName()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> FindMissingMapFile(string missingMapFile)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }


        public void ShowTopologyView()
        {
            throw new NotImplementedException();
        }

    }
}
