using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public class TomlArray : TomlObject
    {
        private readonly List<TomlObject> items = new List<TomlObject>();

        public void Add(TomlObject o)
        {
            this.items.Add(o);
        }

        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }

        public TomlObject this[int index]
        {
            get
            {
                return this.items[index];
            }
        }

        public T Get<T>(int index)
        {
            return this.items[index].Get<T>();
        }

        public override T Get<T>()
        {
            return this.Get<T>(TomlConfig.DefaultInstance);
        }

        public override T Get<T>(TomlConfig config)
        {
            return Converter.Convert<T>(this);
        }

        public override object Get(Type t)
        {
            return this.Get(t, TomlConfig.DefaultInstance);
        }

        public override object Get(Type t, TomlConfig config)
        {
            return Converter.Convert(t, this);
        }

        public IEnumerable<T> To<T>()
        {
            return this.items.Select((to) => to.Get<T>());
        }

        public static TomlArray From(IEnumerable enumerable)
        {
            var a = new TomlArray();

            if (enumerable != null)
            {
                foreach (var e in enumerable)
                {
                    a.Add(TomlValue.From(e, e.GetType()));
                }
            }

            return a;
        }

        public override void WriteTo(StreamWriter sw )
        {
            this.WriteTo(sw, TomlConfig.DefaultInstance);
        }

        public override void WriteTo(StreamWriter writer, TomlConfig config)
        {
            writer.Write("[");

            for (int i = 0; i < this.items.Count - 1; i++)
            {
                this.items[i].WriteTo(writer);
                writer.Write(", ");
            }

            if(this.items.Count > 0)
            {
                this.items[this.items.Count - 1].WriteTo(writer);
            }

            writer.Write("]");
        }
    }
}
