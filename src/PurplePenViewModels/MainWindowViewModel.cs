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
using System.Drawing;
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase, IUserInterface
    {
        Controller controller = null!;
        SymbolDB symbolDB = null!;

        [ObservableProperty]
        MapViewerViewModel mapViewerViewModel = new MapViewerViewModel();

        public void Initialize(Controller controller, SymbolDB symbolDB)
        {
            this.controller = controller;
            this.symbolDB = symbolDB;
        }

        public Size Size => throw new NotImplementedException();


        public void InfoMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void WarningMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void ErrorMessage(string message)
        {
            throw new NotImplementedException();
        }

        public bool OKCancelMessage(string message, bool okDefault)
        {
            throw new NotImplementedException();
        }

        public YesNoCancel YesNoCancelQuestion(string message, bool yesDefault)
        {
            throw new NotImplementedException();
        }

        public bool YesNoQuestion(string message, bool yesDefault)
        {
            throw new NotImplementedException();
        }

        public string GetOpenFileName()
        {
            throw new NotImplementedException();
        }

        public bool FindMissingMapFile(string missingMapFile)
        {
            throw new NotImplementedException();
        }

        public bool GetCurrentLocation(out PointF location, out float pixelSize)
        {
            throw new NotImplementedException();
        }

        public void InitiateMapDragging(PointF initialPos, PointerButton buttonEnd)
        {
            throw new NotImplementedException();
        }

        public int LogicalToDeviceUnits(int value)
        {
            throw new NotImplementedException();
        }

        public YesNoCancel MovingSharedControl(string controlCode, string otherCourses)
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

        public void ShowTopologyView()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Shows the Open File dialog filtered to Purple Pen files (.ppen),
        /// and opens the selected file.
        /// </summary>
        [RelayCommand]
        private async Task FileOpenPurplePenFile()
        {
#if PORTING
            // Not all functionality ported from MainFrame.openMenu_Click.
#endif
            FileOpenSingleViewModel fileOpenVM = new FileOpenSingleViewModel {
                FileFilters = MiscText.OpenFileDialog_PurplePenFilter,
                InitialFileFilterIndex = 1
            };

            bool result = await Services.DialogService.ShowDialogAsync(fileOpenVM);

            if (result && fileOpenVM.SelectedFile != null) {
                string newFilename = fileOpenVM.SelectedFile;
                bool success = controller.LoadNewFile(newFilename);
            }
        }

        /// <summary>
        /// Shows the Add Course dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowAddCourseDialog()
        {
#if PORTING
            // TODO: Initialize ViewModel from current event data (map scale, etc.)
            // and process the result to actually add the course.
#endif
            AddCourseDialogViewModel vm = new AddCourseDialogViewModel();
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with OK button and Information icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxOk()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is an informational message with an OK button.",
                Buttons = MessageBoxButtons.Ok,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Information
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with OK/Cancel buttons and Warning icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxOkCancel()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is a warning message with OK and Cancel buttons.",
                Buttons = MessageBoxButtons.OkCancel,
                DefaultButton = MessageBoxButton.Ok,
                Icon = MessageBoxIcon.Warning
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with Yes/No buttons and Question icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxYesNo()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is a question message with Yes and No buttons. Do you want to proceed?",
                Buttons = MessageBoxButtons.YesNo,
                DefaultButton = MessageBoxButton.Yes,
                Icon = MessageBoxIcon.Question
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Test: shows a message box with Yes/No/Cancel buttons and Error icon.
        /// </summary>
        [RelayCommand]
        private async Task TestMessageBoxYesNoCancel()
        {
            MessageBoxDialogViewModel vm = new MessageBoxDialogViewModel {
                Message = "This is an error message with Yes, No, and Cancel buttons.",
                Buttons = MessageBoxButtons.YesNoCancel,
                DefaultButton = MessageBoxButton.Yes,
                Icon = MessageBoxIcon.Error
            };
            await Services.DialogService.ShowDialogAsync(vm);
        }

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        [RelayCommand]
        private async Task ShowAboutDialog()
        {
            AboutDialogViewModel aboutViewModel = new AboutDialogViewModel();
            await Services.DialogService.ShowDialogAsync(aboutViewModel);
        }

        /// <summary>
        /// Shows the Switch Language dialog and applies the selected language.
        /// </summary>
        [RelayCommand]
        private async Task ShowSwitchLanguageDialog()
        {
            string currentCode = Services.UILanguage.LanguageCode;
            SwitchLanguageDialogViewModel vm = new SwitchLanguageDialogViewModel(currentCode, SwitchLanguageDialogViewModel.CreateDefaultLanguages());
            bool result = await Services.DialogService.ShowDialogAsync(vm);

            if (result && vm.SelectedLanguage != null) {
                Services.UILanguage.LanguageCode = vm.SelectedLanguage.Code;
            }
        }
    }
}
