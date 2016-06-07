using System.Collections.Generic;

namespace Nett.UnitTests.Plugins
{
    public sealed class PluginAConfig
    {
        public static readonly string Key = nameof(PluginAConfig);
        public static readonly string SettingKey = nameof(Setting);

        public int Setting { get; set; }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { nameof(this.Setting), this.Setting },
            };
        }
    }
}
