using System.Diagnostics.CodeAnalysis;

namespace Nett.Tests.Plugins
{
    /// <summary>
    /// Root config that holds properties of type object to map it's sub sections.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class ObjRootConfig
    {
        public const string DefaultSimpleToml = @"
RootSetting = 1

[PluginConfig]
Setting = 1
";
        public const int RootSettingDefaultValue = 1;

        public static readonly string PluginConfigKey = nameof(ObjRootConfig.PluginConfig);

        public int RootSetting { get; set; } = RootSettingDefaultValue;
        public object PluginConfig { get; set; }

        public static ObjRootConfig CreateSimple() => new ObjRootConfig() { PluginConfig = new SimplePluginConfig() };
    }
}
