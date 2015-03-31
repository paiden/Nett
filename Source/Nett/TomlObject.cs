using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlObject
    {
        public abstract T Get<T>();
    }
}
