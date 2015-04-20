using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    internal static class TomlWriter
    {
        public static void WriteTomlArrayTable(StreamWriter writer, TomlArray array, TomlTable parent, string key, TomlConfig config)
        {
            foreach(var e in array.Items)
            {
                writer.WriteLine("[[{0}]]", key);
                writer.write
            }
            throw new NotImplementedException();
        }
    }
}
