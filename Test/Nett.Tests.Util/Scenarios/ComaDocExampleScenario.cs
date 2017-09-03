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
            IConfigSource appSource = null;
            IConfigSource userSource = null;

            // merge both TOML files into one settings object
            var config = Config.CreateAs()
                .MappedToType(() => new AppSettings())
                .StoredAs(store => store
                    .File(appSettings).AccessedBySource("app", out appSource).MergeWith(
                    store.File(userSettings).AccessedBySource("user", out userSource)))
                .UseTomlConfiguration(null)
                .Initialize();

            // Read the settings
            var oldTimeout = config.Get(s => s.IdleTimeout);
            var oldUserName = config.Get(s => s.User.UserName);

            // Save settings. When no override source is given, the system will save back to the file
            // where the setting was loaded from during the merge operation
            config.Set(s => s.User.UserName, oldUserName + "_New");

            // Save setting into user file. User setting will override app setting until the setting
            // gets cleared from the user file
            config.Set(s => s.IdleTimeout, oldTimeout + TimeSpan.FromMinutes(15), appSource);

            // Now clear the user setting again, after that the app setting will be returned when accessing the setting again
            config.Clear(s => s.IdleTimeout, userSource);

            // Now clear the setting without a scope, this will clear it from the currently active source.
            // In this case the setting will be cleared from both files => The setting will not be in any config anymore
            config.Clear(s => s.IdleTimeout);
        }
    }
}
