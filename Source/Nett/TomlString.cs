using System;
using System.Collections.Generic;
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
    }
}
