using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlArray : TomlObject
    {
        private static readonly Type ListType = typeof(IList);
        private static readonly Type ObjectType = typeof(object);
        private readonly List<TomlObject> items = new List<TomlObject>();

        public TomlArray()
        {

        }

        public TomlArray(IEnumerable<TomlValue> values)
        {   
            foreach (var v in values)
            {
                this.Add(v);
            }
        }

        public void Add(TomlObject o)
        {
            this.items.Add(o);
        }

        public int Count => this.items.Count;

        internal bool IsTableArrayInternal() => this.items.Count > 0 && this.items[0].GetType() == Types.TomlTableType;

        public TomlObject this[int index] => this.items[index];

        public T Get<T>(int index) => this.items[index].Get<T>();

        public override T Get<T>() => this.Get<T>(TomlConfig.DefaultInstance);

        public override T Get<T>(TomlConfig config) => Converter.Convert<T>(this);

        public override object Get(Type t) => this.Get(t, TomlConfig.DefaultInstance);

        public override object Get(Type t, TomlConfig config)
        {
            if(t.IsArray)
            {
                var et = t.GetElementType();
                var a = Array.CreateInstance(et, this.items.Count);
                int cnt = 0;
                foreach(var i in this.items)
                {
                    a.SetValue(i.Get(et, config), cnt++);
                }

                return a;
            }


            if(!ListType.IsAssignableFrom(t))
            {
                throw new InvalidOperationException(string.Format("Cannot convert TOML array to '{0}'.", t.FullName));
            }

            var collection = (IList)Activator.CreateInstance(t);
            Type itemType = ObjectType;
            if(t.IsGenericType)
            {
                itemType = t.GetGenericArguments()[0];
            }

            foreach(var i in this.items)
            {
                collection.Add(i.Get(itemType, config));
            }

            return collection;
        }

        public object Last() => this.items[this.items.Count - 1];

        public IEnumerable<T> To<T>() => this.items.Select((to) => to.Get<T>());

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
