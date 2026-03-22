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
using System.Threading.Tasks;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Shows the Open File dialog filtered to Purple Pen files (.ppen),
        /// and opens the selected file.
        /// </summary>
        [RelayCommand]
        private async Task FileOpenPurplePenFile()
        {

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
