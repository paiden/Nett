using System;

namespace Nett.Coma.Tests.TestData
{
    public class TestAppSettings
    {
        public static readonly TestAppSettings GlobalSettings = new TestAppSettings()
        {
            BinDir = @"C:\Program Files\TestApp\bin",
            LicenseKey = "88773372-52CB-49AB-84A6-F7A81F3B8FEA",

            Network = new NetworkSettings()
            {
                ListenPort = 22103,
                Remote = "http://thetestserver.dev"
            },
        };

        public static readonly UserSettings User1Settings = new UserSettings()
        {
            UserName = "User01",
            Sid = 2,
            Theme = "Dark",
        };

        public static readonly UserSettings User2Settings = new UserSettings()
        {
            UserName = "User02",
            Sid = 3,
            Theme = "Blue",
        };

        public string BinDir { get; set; }
        public string LicenseKey { get; set; }
        public DateTime LastUpdateCheck { get; set; } = new DateTime(2000, 01, 01);
        public TimeSpan UpdateCheckInterval { get; set; } = TimeSpan.FromDays(10);

        public NetworkSettings Network { get; set; }
        public UserSettings User { get; set; }

        public sealed class NetworkSettings
        {
            public int ListenPort { get; set; }
            public string Remote { get; set; }
        }

        public sealed class UserSettings
        {
            public string UserName { get; set; }
            public int Sid { get; set; }
            public string Theme { get; set; } = "Default";
        }
    }
}
