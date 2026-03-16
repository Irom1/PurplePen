using System;
using Newtonsoft.Json;

namespace PurplePen.Livelox
{
    class SettingsProvider
    {
        public LiveloxSettings LoadSettings()
        {
            try
            {
                var settings = JsonConvert.DeserializeObject<LiveloxSettings>(
                    System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(UserSettings.Current.LiveloxSettings))
                );
                return settings ?? new LiveloxSettings();
            }
            catch
            {
                return new LiveloxSettings();
            }
        }

        public void SaveSettings(LiveloxSettings liveloxSettings)
        {
            UserSettings.Current.LiveloxSettings = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(liveloxSettings)));
            UserSettings.Current.Save();
        }
    }
}