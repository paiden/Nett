using System;

namespace Nett
{
    public sealed partial class TomlConfig
    {
        private sealed class ConfigScope : IDisposable
        {
            private readonly TomlConfig previousConfig;

            public ConfigScope(TomlConfig previousConfig)
            {
                this.previousConfig = previousConfig;
            }

            void IDisposable.Dispose()
            {
                UseNewDefaultConfig(previousConfig);
            }
        }
    }
}
