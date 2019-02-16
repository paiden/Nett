using System.IO;
using Microsoft.Extensions.Configuration;
using Nett.Extensions;

namespace Nett.AspNet
{
    public sealed class TomlConfigurationProvider : FileConfigurationProvider
    {
        public TomlConfigurationProvider(TomlConfigurationSource source)
            : base(source)
        {
            source.CheckNotNull(nameof(source));
        }

        public override void Load(Stream stream)
        {
            var table = Toml.ReadStream(stream);
            this.Data = ProviderDictionaryConverter.ToProviderDictionary(table);
        }
    }
}
