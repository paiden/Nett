using System;
using System.IO;
using Nett.Coma;

namespace Nett.Tests.Util.Scenarios
{
    // This code is only used to validate documentation code is compileable ATM
    class ComaDocExampleScenario : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public class AppSettings
        {
            public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(15);

            public UserSettings User { get; set; } = new UserSettings();

            public class UserSettings
            {
                public string UserName { get; set; }
            }
        }

        public static void Setup()
        {
            var appSettings = "%APPDATA%/AppSettings.toml";
            var userSettings = "%USERDATA%/UserSettings.toml";

            // Merge only works when files exist on disk, user has to do the initial creation manually
            File.WriteAllText(appSettings, "IdleTimeout = 00:15:00");
            File.WriteAllText(userSettings,
@"
[User] 
UserName = ""Test""
");

            // Prepare sources for merging
            var appSource = ConfigSource.CreateFileSource(appSettings);
            var userSource = ConfigSource.CreateFileSource(userSettings);
            var merged = ConfigSource.Merged(appSource, userSource); // order important here

            // merge both TOML files into one settings object
            var settings = Config.Create(() => new AppSettings(), merged);

            // Read the settings
            var oldTimeout = settings.Get(s => s.IdleTimeout);
            var oldUserName = settings.Get(s => s.User.UserName);

            // Save settings. When no override source is given, the system will save back to the file
            // where the setting was loaded from during the merge operation
            settings.Set(s => s.User.UserName, oldUserName + "_New");

            // Save setting into user file. User setting will override app setting until the setting
            // gets cleared from the user file
            settings.Set(s => s.IdleTimeout, oldTimeout + TimeSpan.FromMinutes(15), userSource);

            // Now clear the user setting again, after that the app setting will be returned when accessing the setting again
            settings.Clear(s => s.IdleTimeout, userSource);

            // Now clear the setting without a scope, this will clear it from the currently active source.
            // In this case the setting will be cleared from both files => The setting will not be in any config anymore
            settings.Clear(s => s.IdleTimeout);
        }
    }
}
