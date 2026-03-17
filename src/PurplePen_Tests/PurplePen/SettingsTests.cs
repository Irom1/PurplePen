using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestingUtils;

namespace PurplePen.Tests
{
    [TestClass]
    public class SettingsTests: TestFixtureBase
    {
        [TestMethod, DoNotParallelize]
        public void Settings1()
        {
            string nonExistant = TestUtil.GetTestFile("settings\\nosettings.json");

            // Remember the old settings path, so we can restore it after the test.
            string oldSettingsPath = UserSettings.SettingsPath;
            UserSettings.Current = null;   // Remove settings from memory.

            try {
                File.Delete(nonExistant);
                Assert.IsFalse(File.Exists(nonExistant));

                UserSettings.Initialize(nonExistant);
                Assert.IsNull(UserSettings.Current.UILanguage);
                Assert.IsNull(UserSettings.Current.LastLoadedFile);
                Assert.AreEqual(0.7F, UserSettings.Current.MapIntensity);
                Assert.AreEqual(true, UserSettings.Current.ShowPrintArea);
                Assert.AreNotEqual(Guid.Empty, UserSettings.Current.ClientId);

                UserSettings.Current.LastLoadedFile = "C:\\lastfile.ppen";
                UserSettings.Current.MapIntensity = 0.9F;
                UserSettings.Current.ShowPrintArea = false;

                UserSettings.Current.Save();
                Assert.IsTrue(File.Exists(nonExistant));

                UserSettings.Current = null;   // Remove settings from memory.

                UserSettings.Initialize(nonExistant);
                Assert.IsNull(UserSettings.Current.UILanguage);
                Assert.AreEqual("C:\\lastfile.ppen", UserSettings.Current.LastLoadedFile);
                Assert.AreEqual(0.9F, UserSettings.Current.MapIntensity);
                Assert.AreEqual(false, UserSettings.Current.ShowPrintArea);
                Assert.AreEqual("2017", UserSettings.Current.NewEventMapStandard);

                File.Delete(nonExistant);
            }
            finally {
                // Restore the old settings so other tests still work.
                UserSettings.Current = null;
                UserSettings.Initialize(oldSettingsPath);
            }
        }

    }
}
