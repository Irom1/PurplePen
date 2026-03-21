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

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the main window. Contains a counter value and
    /// commands to increment and decrement it.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
    }
}
