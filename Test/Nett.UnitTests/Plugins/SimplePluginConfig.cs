using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nett.UnitTests.Plugins
{
    /// <summary>
    /// Simple sub config. This should be used to validate basic plugin mapping techniques.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class SimplePluginConfig
    {
        public const int SettingDefaultValue = 1;

        public static readonly string Key = nameof(SimplePluginConfig);
        public static readonly string SettingKey = nameof(Setting);

        public int Setting { get; set; } = SettingDefaultValue;

        public static SimplePluginConfig CreateDefault() => new SimplePluginConfig();

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { nameof(this.Setting), this.Setting },
            };
        }

        public override bool Equals(object obj)
        {
            var other = (SimplePluginConfig)obj;

            return this.Setting == other.Setting;
        }
    }
}
