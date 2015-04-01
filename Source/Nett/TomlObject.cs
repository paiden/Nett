using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlObject
    {
        public abstract T Get<T>();
        public abstract object Get(Type t);
        public abstract void WriteTo(StreamWriter writer);
    }
}
