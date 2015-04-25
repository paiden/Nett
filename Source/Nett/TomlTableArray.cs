using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlTableArray : TomlArray
    {
        public string Name { get; set; }

        public TomlTableArray(string name)
            : base()
        {
            this.Name = name;
        }
        public TomlTableArray(string tableName, IEnumerable enumerable, TomlConfig config)
            : base(enumerable, config)
        {

        }

        public override void WriteTo(StreamWriter writer, TomlConfig config)
        {
            foreach(var i in this.items)
            {
                writer.WriteLine("[[{0}]]", this.Name);
                i.WriteTo(writer);
                writer.WriteLine();
            }
        }
    }
}
