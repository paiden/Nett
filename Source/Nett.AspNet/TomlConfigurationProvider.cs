using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Nett.Extensions;

namespace Nett.AspNet
{
    public sealed class TomlConfigurationProvider : FileConfigurationProvider
    {
        public bool CaseInsensitiveKeys { get; }

        public TomlConfigurationProvider(TomlConfigurationSource source)
            : base(source)
        {
            source.CheckNotNull(nameof(source));

            this.CaseInsensitiveKeys = source.CaseInsensitiveKeys;
        }

        public override void Load(Stream stream)
        {
            var table = Toml.ReadStream(stream);

            if (this.CaseInsensitiveKeys)
            {
                this.Data = ProviderDictionaryConverter.ToProviderDictionary(table, StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                this.Data = ProviderDictionaryConverter.ToProviderDictionary(table);
            }
        }
    }
}
