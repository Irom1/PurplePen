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
        // The [ObservableProperty] attribute tells the MVVM Toolkit source generator
        // to create a public property named "Counter" (capital C) backed by this field.
        // The generated property automatically calls OnPropertyChanged() when set,
        // which notifies the UI to update any bindings that reference "Counter".
        //
        // Naming convention: the field must be camelCase (or _camelCase).
        // The generator strips the underscore/lowercases and creates a PascalCase property.
        //   field: counter  -->  generated property: Counter
        [ObservableProperty]
        private int counter = 0;

        // The [RelayCommand] attribute tells the source generator to create an ICommand
        // property named "IncrementCounterCommand" (method name + "Command" suffix).
        // In the XAML, we bind a Button's Command property to "IncrementCounterCommand".
        //
        // When the button is clicked, Avalonia invokes the command, which calls this method.
        // Because we set Counter (the generated property), the UI automatically updates.
        [RelayCommand]
        private void IncrementCounter()
        {
            Counter++;
        }

        // Same pattern: generates a "DecrementCounterCommand" ICommand property.
        [RelayCommand]
        private void DecrementCounter()
        {
            Counter--;
        }
    }
}
