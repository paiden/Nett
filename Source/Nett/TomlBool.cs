using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        public TomlBool(bool value)
            : base(value)
        {

        }

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
