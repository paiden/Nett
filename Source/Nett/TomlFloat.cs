using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlFloat : TomlValue<double>
    {
        public TomlFloat(double value)
            : base(value)
        {

        }

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
