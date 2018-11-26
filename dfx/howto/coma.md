# Nett.Coma

'Nett.Coma' is an extension library for Nett. The purpose of 'Coma' is to provide a modern and powerful config system
in the .Net ecosytem. 

Since .Net 2.0 there exists a .Net framework integrated configuration system inside the `System.Configuration` namespace.
So why a new config system? Because of the following disadvantages, that the .Net integrated config system has:

+ XML as configuration format
+ Very much boilerplate code needed to get strongly typed config objects
+ No way to support for advanced use case scenarios (e.g. multi file configurations)
+ Not actively maintained

'Coma' attempts to solve many of these pitfalls by providing the following features:

+ TOML used as the configuration format
+ Coma wraps plain CLR config type to provide additional functionality
+ Out of the box support for multi file / merge configurations (e.g. like Git config system)

## Getting started

Assume the following settings object is used inside an app
```C#
public class AppSettings
{
    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(15);

    public UserSettings User { get; set; } = new UserSettings();

    public class UserSettings
    {
        public string UserName { get; set; }
    }
}
 ```

The following example should give an idea how to integrate the 'Coma' system to get a
application settings implementation.

```C#
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
```