using System;
using System.Diagnostics;
using System.IO;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests
{
    public abstract class TestsBase
    {
        protected static void ModifyFileOnDisk(string fileName, Action<SingleLevelConfig> modify)
        {
            var read = Toml.ReadFile<SingleLevelConfig>(fileName);
            modify(read);
            Toml.WriteFile(read, fileName);
        }

        protected static void TryDeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Failed to cleanup file:" + exc.ToString());
            }
        }

        protected static void CreateMergedTestAppConfig(out string mainFile, out string userFile)
        {
            mainFile = "initMainSettings".TestRunUniqueName(Toml.FileExtension);
            userFile = "initUserSettings".TestRunUniqueName(Toml.FileExtension);

            var main = TestData.TestAppSettings.GlobalSettings;
            var userSettings = TestData.TestAppSettings.User1Settings;
            var user = Toml.Create();
            user.Add(nameof(main.User), userSettings);

            // Act
            Toml.WriteFile(main, mainFile);
            Toml.WriteFile(user, userFile);
        }
    }
}
