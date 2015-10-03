using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nett
{
    public sealed class TomlArray : TomlObject
    {
        private static readonly Type TomlArrayType = typeof(TomlArray);
        private static readonly Type ListType = typeof(IList);
        private static readonly Type ObjectType = typeof(object);
        private readonly List<TomlObject> items = new List<TomlObject>();
        internal List<TomlObject> Items => this.items;

        public override string ReadableTypeName => "array";

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

        public override object Get(Type t, TomlConfig config)
        {
            if (t == TomlArrayType) { return this; }

            if (t.IsArray)
            {
                var et = t.GetElementType();
                var a = Array.CreateInstance(et, this.items.Count);
                int cnt = 0;
                foreach (var i in this.items)
                {
                    a.SetValue(i.Get(et, config), cnt++);
                }

                return a;
            }


            if (!ListType.IsAssignableFrom(t))
            {
                throw new InvalidOperationException(string.Format("Cannot convert TOML array to '{0}'.", t.FullName));
            }

            var collection = (IList)Activator.CreateInstance(t);
            Type itemType = ObjectType;
            if (t.IsGenericType)
            {
                itemType = t.GetGenericArguments()[0];
            }

            foreach (var i in this.items)
            {
                collection.Add(i.Get(itemType, config));
            }

            return collection;
        }

        public object Last() => this.items[this.items.Count - 1];

        public IEnumerable<T> To<T>() => this.To<T>(TomlConfig.DefaultInstance);
        public IEnumerable<T> To<T>(TomlConfig config) => this.items.Select((to) => to.Get<T>(config));

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
