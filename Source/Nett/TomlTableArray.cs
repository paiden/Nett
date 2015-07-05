using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlTableArray : TomlObject
    {
        private static readonly Type ListType = typeof(IList);
        private static readonly Type ObjectType = typeof(object);
        private static readonly Type TableArrayType = typeof(TomlTableArray);

        private readonly List<TomlTable> items = new List<TomlTable>();
        public List<TomlTable> Items => this.items;

        public TomlTableArray()
            : this(null)
        {
        }

        public TomlTableArray(IEnumerable<TomlTable> enumerable)
        {
            if (enumerable != null)
            {
                foreach (var e in enumerable)
                {
                    this.Add(e);
                }
            }
        }

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public int Count => this.items.Count;

        public void Add(TomlTable table)
        {
            this.items.Add(table);
        }

        public TomlTable this[int index] => this.items[index];

        public override object Get(Type t, TomlConfig config)
        {
            if(t == TableArrayType) { return this; }

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
    }
}
