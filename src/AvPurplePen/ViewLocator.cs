using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PurplePen.ViewModels;

namespace AvPurplePen
{
    /// <summary>
    /// A convention-based IDataTemplate that automatically finds and creates
    /// the right View for a given ViewModel.
    ///
    /// HOW IT WORKS:
    /// Avalonia's template system calls Match() whenever it needs to display a
    /// non-control object (like a ViewModel). If Match() returns true, Avalonia
    /// then calls Build() to get the actual UI control to display.
    ///
    /// The convention is a name-based mapping:
    ///   ViewModel class:  PurplePen.ViewModels.FooViewModel
    ///   View class:       AvPurplePen.Views.FooView
    ///
    /// WHEN IS THIS USED?
    /// It's used when a ViewModel instance appears as the Content of a
    /// ContentControl (or similar). For example, if you set:
    ///     myContentControl.Content = new SomeViewModel();
    /// Avalonia will use this ViewLocator to resolve it to the matching View.
    ///
    /// It is NOT used for MainWindow, because that is created directly in
    /// App.axaml.cs — no automatic resolution is needed there.
    ///
    /// WHY THE ASSEMBLY LOOKUP?
    /// The ViewModels live in a separate assembly (PurplePenViewModels), while
    /// the Views live in this assembly (AvPurplePen). Type.GetType() only
    /// searches the calling assembly by default, so we explicitly search
    /// the AvPurplePen assembly to find View types.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        // Cache a reference to the assembly containing the Views (this assembly).
        private static readonly Assembly ViewAssembly = typeof(ViewLocator).Assembly;

        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            // Convert e.g. "PurplePen.ViewModels.MainWindowViewModel"
            //       to      "AvPurplePen.Views.MainWindowView"
            string viewModelName = param.GetType().FullName!;
            string viewName = viewModelName
                .Replace("PurplePen.ViewModels", "AvPurplePen.Views", StringComparison.Ordinal)
                .Replace("ViewModel", "View", StringComparison.Ordinal);

            // Search for the View type in this assembly (where Views are defined).
            Type? type = ViewAssembly.GetType(viewName);

            if (type != null) {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + viewName };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
