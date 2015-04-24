using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        public TomlInt(long value)
            : base(value)
        {

        }
    }
}
