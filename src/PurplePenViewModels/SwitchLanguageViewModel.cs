// SwitchLanguageViewModel.cs
//
// ViewModel for the language-switching dialog. Holds the currently selected
// language code and a list of available languages to choose from.
// Each language is represented by a LanguageItem with a code (e.g. "fr")
// and a display name (e.g. "Français").

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Represents a single language choice in the language list.
    /// </summary>
    public class LanguageItem
    {
        /// <summary>
        /// The culture code, e.g. "en", "fr", "pt-BR".
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// The display name shown in the list, e.g. "English", "Français".
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Creates a new LanguageItem with the given culture code and display name.
        /// </summary>
        /// <param name="code">Culture code such as "en" or "pt-BR".</param>
        /// <param name="displayName">Human-readable language name.</param>
        public LanguageItem(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }

        /// <summary>
        /// Returns the display name for use in list controls.
        /// </summary>
        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// ViewModel for the Switch Language dialog. Contains the list of
    /// available languages and tracks which one is currently selected.
    /// </summary>
    public partial class SwitchLanguageViewModel : ViewModelBase
    {
        /// <summary>
        /// The list of languages available for selection.
        /// </summary>
        public ObservableCollection<LanguageItem> AvailableLanguages { get; }

        /// <summary>
        /// The currently selected language item in the list.
        /// Bound two-way to the ListBox's SelectedItem.
        /// </summary>
        [ObservableProperty]
        private LanguageItem? selectedLanguage;

        /// <summary>
        /// Parameterless constructor for the Avalonia designer.
        /// Populates the ViewModel with sample data so the previewer
        /// can render the dialog with realistic content.
        /// </summary>
        public SwitchLanguageViewModel()
            : this("en", CreateSampleLanguages())
        {
        }

        /// <summary>
        /// Creates a new SwitchLanguageViewModel with the specified current
        /// language and list of available languages.
        /// </summary>
        /// <param name="currentLanguageCode">The language code currently in use, e.g. "en".</param>
        /// <param name="availableLanguages">The list of languages to offer.</param>
        public SwitchLanguageViewModel(string currentLanguageCode, ObservableCollection<LanguageItem> availableLanguages)
        {
            AvailableLanguages = availableLanguages;

            // Select the item matching the current language code.
            foreach (LanguageItem item in AvailableLanguages) {
                if (string.Equals(item.Code, currentLanguageCode, System.StringComparison.OrdinalIgnoreCase)) {
                    SelectedLanguage = item;
                    break;
                }
            }
        }

        /// <summary>
        /// Creates a sample list of languages for design-time and prototyping.
        /// </summary>
        /// <returns>A collection of sample LanguageItems.</returns>
        private static ObservableCollection<LanguageItem> CreateSampleLanguages()
        {
            return new ObservableCollection<LanguageItem> {
                new LanguageItem("en", "English"),
                new LanguageItem("fr", "Français"),
                new LanguageItem("de", "Deutsch"),
                new LanguageItem("es", "Español"),
                new LanguageItem("it", "Italiano"),
                new LanguageItem("ja", "日本語"),
                new LanguageItem("pt", "Português"),
                new LanguageItem("pt-BR", "Português (Brasil)"),
                new LanguageItem("zh-CN", "中文 (简体)"),
                new LanguageItem("zh-TW", "中文 (繁體)"),
                new LanguageItem("ko", "한국어"),
                new LanguageItem("ru", "Русский"),
                new LanguageItem("sv", "Svenska"),
                new LanguageItem("fi", "Suomi"),
                new LanguageItem("nb", "Norsk (Bokmål)"),
                new LanguageItem("da", "Dansk"),
            };
        }

        /// <summary>
        /// Creates test data for design-time and prototyping.
        /// </summary>
        /// <returns>A SwitchLanguageViewModel populated with sample languages.</returns>
        public static SwitchLanguageViewModel CreateTestData()
        {
            return new SwitchLanguageViewModel("en", CreateSampleLanguages());
        }
    }
}
