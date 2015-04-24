using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        public TomlTimeSpan(TimeSpan value)
            : base(value)
        {
        }
    }
}
