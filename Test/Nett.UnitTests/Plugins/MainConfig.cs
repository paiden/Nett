using System.Collections.Generic;

namespace Nett.UnitTests.Plugins
{
    public class MainConfig
    {
        public string Setting { get; set; } = "";

        public Dictionary<string, object> PluginConfigs { get; set; } = new Dictionary<string, object>();
    }
}
