using System.Collections.Generic;

namespace Nett.UnitTests.Plugins
{
    public class DictRootConfig
    {
        public string Setting { get; set; } = "";

        public Dictionary<string, object> PluginConfigs { get; set; } = new Dictionary<string, object>();

        public static DictRootConfig CreateMainConfigWithSimpleSubConfig()
        {
            var cfg = new DictRootConfig();
            cfg.PluginConfigs.Add(SimplePluginConfig.Key, new SimplePluginConfig());
            return cfg;
        }
    }
}
