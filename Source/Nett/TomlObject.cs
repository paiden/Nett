using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    enum TomlObjectType
    {
        Array,
        Value,
    }

    public abstract class TomlObject
    {
        internal List<TomlComment> Comments { get; private set; }
        public abstract T Get<T>();
        public abstract T Get<T>(TomlConfig config);
        public abstract object Get(Type t);
        public abstract object Get(Type t, TomlConfig config);
        public abstract void WriteTo(StreamWriter writer);
        public abstract void WriteTo(StreamWriter writer, TomlConfig config);

        public TomlObject()
        {
            this.Comments = new List<TomlComment>();
        }
    }
}
