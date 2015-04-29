using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlString : TomlValue<string>
    {
        public enum TypeOfString
        {
            Default,
            Normal,
            Literal,
            Multiline,
            MultilineLiteral,
        }

        private TypeOfString type = TypeOfString.Default;

        public TomlString(string value, TypeOfString type = TypeOfString.Default)
            : base(value)
        {
            this.type = type;
        }

        public override void WriteTo(StreamWriter writer, TomlConfig config)
        {
            if(this.type == TypeOfString.Default)
            {
                var toWrite = this.Value.Escape();
                writer.Write("\"{0}\"", toWrite);
            }
        }
    }
}
