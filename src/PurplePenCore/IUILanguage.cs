// IUILanguage.cs
//
// Service interface for getting and setting the UI language.
// Setting the LanguageCode property changes CurrentUICulture and
// triggers any necessary UI refresh (e.g. localized binding updates).

namespace PurplePen
{
    /// <summary>
    /// Provides access to the current UI language and allows changing it at runtime.
    /// Setting <see cref="LanguageCode"/> updates the thread's CurrentUICulture and
    /// notifies the UI layer to refresh localized strings.
    /// </summary>
    public interface IUILanguage
    {
        /// <summary>
        /// Gets or sets the current UI language code (e.g. "en", "fr", "de").
        /// Setting this property changes CurrentUICulture on the current thread
        /// and triggers a refresh of all localized UI bindings.
        /// </summary>
        string LanguageCode { get; set; }
    }
}
