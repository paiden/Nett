namespace Nett.UnitTests.Plugins
{
    /// <summary>
    /// For plugin B it is essential that it doesn't use any TOML specific types. With class should simulate the case,
    /// where the assembly should not take any dependency to TOML but still be able to get written and read correctly and with all
    /// features form a TOMl file.
    /// </summary>
    public sealed class PluginBConfig
    {
        public static readonly string Key = nameof(PluginBConfig);

        public float Settging { get; set; }
    }
}
