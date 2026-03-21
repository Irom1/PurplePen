// AboutDialogViewModel.cs
//
// ViewModel for the About dialog. Provides the formatted version string
// and bitness/distribution text as read-only properties for binding.
// Localized format strings (e.g. "Version {0}") belong in the View layer
// and are applied via XAML StringFormat bindings.

using System;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// ViewModel for the About dialog. Exposes the application version
    /// and bitness information as bindable properties.
    /// </summary>
    public class AboutDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// The pretty-printed version number (e.g. "3.5.5" or "3.5.5 Beta 2").
        /// Does not include the "Version" prefix — that is applied by the View
        /// using a localized StringFormat.
        /// </summary>
        public string PrettyVersion { get; }

        /// <summary>
        /// Describes the process bitness and distribution type,
        /// e.g. "64-bit (Standalone Setup)" or "32-bit (Windows Store)".
        /// </summary>
        public string BitnessText { get; }

        /// <summary>
        /// Creates a new AboutDialogViewModel with version and bitness info
        /// computed from the running environment.
        /// </summary>
        public AboutDialogViewModel()
        {
            PrettyVersion = Util.PrettyVersionString(VersionNumber.Current);

            string bitness = Environment.Is64BitProcess ? MiscText.SixtyfourBit : MiscText.ThirtytwoBit;
#if MSSTORE
            bitness += " (" + MiscText.WindowsStore + ")";
#else
            bitness += " (" + MiscText.StandaloneSetup + ")";
#endif
            BitnessText = bitness;
        }
    }
}
