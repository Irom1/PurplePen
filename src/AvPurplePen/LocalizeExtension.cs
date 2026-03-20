// LocalizeExtension.cs
//
// A XAML markup extension that provides localized strings with two behaviors:
//
//   - DESIGN TIME: Returns the English string directly from UIText.ResourceManager,
//     so the Avalonia previewer in Visual Studio shows real text while editing XAML.
//
//   - RUNTIME: Creates a binding to a LocalizedString.Value property, which supports
//     live language switching. When Loc.NotifyLanguageChanged() is called, all
//     LocalizedString instances raise PropertyChanged and controls update automatically.
//
// XAML usage:
//   <Button Content="{resx:Localize IncrementButton}"/>
//
// where "IncrementButton" is a key in UIText.resx.

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace AvPurplePen
{
    /// <summary>
    /// Markup extension that resolves a localized string by resource key.
    /// At design time, returns the default (English) string for previewer support.
    /// At runtime, returns a binding to a <see cref="LocalizedString"/> for live
    /// language switching.
    /// </summary>
    public class LocalizeExtension : MarkupExtension
    {
        // The resource manager used to look up localized strings.
        static internal readonly ResourceManager resourceManager = UIText.ResourceManager;

        /// <summary>
        /// The resource key to look up in UIText.resx (e.g. "IncrementButton").
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Creates a new LocalizeExtension for the given resource key.
        /// </summary>
        /// <param name="key">The resource key in UIText.resx.</param>
        public LocalizeExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Returns the localized value. At design time, returns the English string
        /// directly. At runtime, creates a LocalizedString and returns a binding
        /// to its Value property that updates when the language changes.
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Design.IsDesignMode) {
                // Design time: return the English string so the previewer shows real text.
                return resourceManager.GetString(Key) ?? $"[{Key}]";
            }

            // Runtime: create a LocalizedString wrapper and bind to its Value property.
            // When Loc.NotifyLanguageChanged() is called, the wrapper raises
            // PropertyChanged for Value, causing the bound control to update.
            LocalizedString source = LocalizedStringManager.Instance.GetLocalizedString(Key);
            Binding binding = new Binding(nameof(LocalizedString.Value))
            {
                Source = source,
                Mode = BindingMode.OneWay
            };

            return binding;
        }
    }

    /// <summary>
    /// Holds a single localized string that can be bound to via its Value property.
    /// When the language changes, <see cref="LocalizedStringManager"/> calls <see cref="Refresh"/>
    /// to raise PropertyChanged and update the bound control.
    /// </summary>
    public class LocalizedString : INotifyPropertyChanged
    {
        private readonly string _key;

        /// <summary>
        /// Creates a new LocalizedString for the given resource key.
        /// </summary>
        /// <param name="key">The resource key in UIText.resx.</param>
        public LocalizedString(string key)
        {
            _key = key;
        }

        /// <summary>
        /// The localized string value for the current UI culture.
        /// Bound to by controls via LocalizeExtension.
        /// </summary>
        public string Value =>
            LocalizeExtension.resourceManager.GetString(_key, CultureInfo.CurrentUICulture) ?? $"[{_key}]";

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged for Value, causing bound controls to re-read the string.
        /// </summary>
        internal void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }

    /// <summary>
    /// Singleton that manages <see cref="LocalizedString"/> instances and
    /// refreshes them all when the UI language changes.
    /// </summary>
    public class LocalizedStringManager
    {
        /// <summary>
        /// The single shared instance.
        /// </summary>
        public static LocalizedStringManager Instance { get; } = new LocalizedStringManager();

        private readonly Dictionary<string, LocalizedString> _strings = new Dictionary<string, LocalizedString>();

        /// <summary>
        /// Gets or creates a LocalizedString for the given resource key.
        /// All controls using the same key share a single instance, so
        /// opening and closing dialogs does not leak memory.
        /// </summary>
        /// <param name="key">The resource key in UIText.resx.</param>
        /// <returns>A bindable LocalizedString whose Value property returns the localized text.</returns>
        public LocalizedString GetLocalizedString(string key)
        {
            if (!_strings.TryGetValue(key, out LocalizedString? localizedString)) {
                localizedString = new LocalizedString(key);
                _strings[key] = localizedString;
            }

            return localizedString;
        }

        /// <summary>
        /// Call this after changing CurrentUICulture to refresh all localized bindings.
        /// </summary>
        public void NotifyLanguageChanged()
        {
            foreach (LocalizedString s in _strings.Values) {
                s.Refresh();
            }
        }

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// </summary>
        private LocalizedStringManager()
        {
        }
    }

}
