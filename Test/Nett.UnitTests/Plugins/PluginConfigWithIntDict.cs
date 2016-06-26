using System.Collections.Generic;
using Nett.UnitTests.Util;

namespace Nett.UnitTests.Plugins
{
    public sealed class PluginConfigWithIntDict
    {
        public static readonly string Key = nameof(PluginConfigWithIntDict);

        public const string PortAKey = "PortA";
        public const string PortBKey = "PortB";

        public const int PortAValue = 1;
        public const int PortBValue = 2;

        public const string TomlDefaultConstructed = "";

        public Dictionary<string, int> Ports { get; set; } = new Dictionary<string, int>();

        public PluginConfigWithIntDict()
        {
            Ports.Add(PortAKey, PortAValue);
            Ports.Add(PortBKey, PortBValue);
        }

        public override bool Equals(object obj) => ((PluginConfigWithIntDict)obj).Ports.ContentEquals(this.Ports);
        public override int GetHashCode() => 0;
    }
}
