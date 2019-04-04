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
            ProcessTable(dict, table, keyPrefix: string.Empty);
            return dict;
        }

        private static void ProcessTable(Dictionary<string, string> dict, TomlTable table, string keyPrefix)
        {
            foreach (var r in table.Rows)
            {
                switch (r.Value)
                {
                    case TomlTableArray ta: ProcessTableArray(dict, ta, FullKey(r)); break;
                    case TomlTable t: ProcessTable(dict, t, keyPrefix + r.Key + ":"); break;
                    case TomlArray a: ProcessArray(dict, a, FullKey(r)); break;
                    case TomlBool b: AddEntry(r, b.Value.ToString(CultureInfo.InvariantCulture)); break;
                    case TomlString s: AddEntry(r, s.Value); break;
                    case TomlInt i: AddEntry(r, i.Value.ToString(CultureInfo.InvariantCulture)); break;
                    case TomlFloat f: AddEntry(r, f.Value.ToString(CultureInfo.InvariantCulture)); break;
                    case TomlOffsetDateTime dt: AddEntry(r, dt.Value.ToString(CultureInfo.InvariantCulture)); break;
                    case TomlDuration ts: AddEntry(r, ts.Value.ToString()); break;
                    default:
                        throw new InvalidOperationException(
                            $"AspNet provider cannot handle TOML object of type '{r.Value.ReadableTypeName}'. " +
                            $"The objects key is '{FullKey(r)}'.");
                }
            }

            void AddEntry(KeyValuePair<string, TomlObject> row, string val)
            {
                dict[FullKey(row)] = val;
            }

            string FullKey(KeyValuePair<string, TomlObject> row)
                => keyPrefix + row.Key;
        }

        private static void ProcessTableArray(Dictionary<string, string> dict, TomlTableArray tableArray, string keyPrefix)
        {
            for (int i = 0; i < tableArray.Items.Count; i++)
            {
                TomlTable table = tableArray.Items[i];
                ProcessTable(dict, table, $"{keyPrefix}:{i}:");
            }
        }

        private static void ProcessArray(Dictionary<string, string> dict, TomlArray array, string fullKey)
        {
            for (int i = 0; i < array.Items.Length; i++)
            {
                if (array.Items[i] is TomlArray a)
                {
                    throw new InvalidOperationException(
                        $"AspNet provider cannot handle jagged arrays, only simple arrays are supported." +
                        $"The arrays key is '{fullKey}'.");
                }

                dict[$"{fullKey}:{i}"] = array.Items[i].ToString();
            }
        }
    }
}
