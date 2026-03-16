using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    public class UserSettings
    {
        public string UILanguage;
        public string LastLoadedFile;
        public float MapIntensity = 0.7F;
        public bool MapHighQuality = true;
        public bool ShowPopupInfo = true;
        public Guid ClientId = new Guid();
        public bool ViewAllControls = false;
        public bool ShowPrintArea = true;
        public string DefaultDescriptionLanguage;
        public string NewEventMapStandard = "2017";
        public string NewEventDescriptionStandard = "2018";
        public string LiveloxSettings;

        public static readonly UserSettings Current = new UserSettings();

        public void Save()
        {
#if !PORTING
            // Handle loading and saving of user settings.
#endif
        }

        public void Load()
        {
#if !PORTING
            // Handle loading and saving of user settings.
#endif
        }
    }
}
