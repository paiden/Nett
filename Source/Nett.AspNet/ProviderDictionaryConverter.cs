using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nett.AspNet
{
    internal static class ProviderDictionaryConverter
    {
        public static Dictionary<string, string> ToProviderDictionary(TomlTable table)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            CreateProviderDictionary(dict, table, keyPrefix: string.Empty);
            return dict;
        }

        private static void CreateProviderDictionary(Dictionary<string, string> dict, TomlTable table, string keyPrefix)
        {
            foreach (var r in table.Rows)
            {
                string sv = string.Empty;
                switch (r.Value)
                {
                    case TomlTable t:
                        CreateProviderDictionary(dict, t, keyPrefix + r.Key + ":");
                        continue;
                    case TomlBool b: sv = b.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlString s: sv = s.Value; break;
                    case TomlInt i: sv = i.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlFloat f: sv = f.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlOffsetDateTime dt: sv = dt.Value.ToString(CultureInfo.InvariantCulture); break;
                    case TomlDuration ts: sv = ts.Value.ToString(); break;
                    default: throw new InvalidOperationException("Unexpected");
                }

                dict[keyPrefix + r.Key] = sv;
            }
        }
    }
}
