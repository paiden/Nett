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
        public abstract T Get<T>(TomlConfig config);
        public abstract object Get(Type t);
        public abstract object Get(Type t, TomlConfig config);
        public abstract void WriteTo(StreamWriter writer);
        public abstract void WriteTo(StreamWriter writer, TomlConfig config);
    }
}
