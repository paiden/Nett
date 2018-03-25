using System;
using System.Collections.Generic;
using System.Globalization;
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
            this.Data = this.ToProviderDictionary(table);
        }

        private Dictionary<string, string> ToProviderDictionary(TomlTable table)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            this.CreateProviderDictionary(dict, table, keyPrefix: string.Empty);
            return dict;
        }

        private void CreateProviderDictionary(Dictionary<string, string> dict, TomlTable table, string keyPrefix)
        {
            foreach (var r in table.Rows)
            {
                string sv = string.Empty;
                switch (r.Value)
                {
                    case TomlTable t: this.CreateProviderDictionary(dict, t, r.Key + ":"); break;
                    case TomlBool b: sv = b.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlString s: sv = s.Value; break;
                    case TomlInt i: sv = i.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlFloat f: sv = f.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlDateTime dt: sv = dt.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlDuration ts: sv = ts.Value.ToString(); break;
                    default: throw new InvalidOperationException("Unexpected");
                }

                dict[keyPrefix + r.Key] = sv;
            }
        }
    }
}
