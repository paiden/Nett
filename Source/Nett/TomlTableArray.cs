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

        public string Name { get; set; }

        public TomlTableArray(string tableName)
        {
            this.Name = tableName;
        }

        public TomlTableArray(string tableName, IEnumerable<TomlTable> enumerable, TomlConfig config)
        {
            this.Name = tableName;

            if (enumerable != null)
            {
                foreach (var e in enumerable)
                {
                    this.Add(e);
                }
            }
        }

        public override bool IsTableArray
        {
            get
            {
                return true;
            }
        }

        public int Count { get { return this.items.Count; } }

        public void Add(TomlTable table)
        {
            this.items.Add(table);
        }

        public override T Get<T>()
        {
            return this.Get<T>(TomlConfig.DefaultInstance);
        }

        public override T Get<T>(TomlConfig config)
        {
            return (T)this.Get(typeof(T), config);
        }

        public override object Get(Type t)
        {
            return this.Get(t, TomlConfig.DefaultInstance);
        }

        public TomlTable this[int index]
        {
            get
            {
                return this.items[index];
            }
        }

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

        public object Last()
        {
            return this.items[this.items.Count - 1];
        }

        public override void WriteTo(StreamWriter writer)
        {
            this.WriteTo(writer, TomlConfig.DefaultInstance);
        }

        public override void WriteTo(StreamWriter writer, TomlConfig config)
        {
            foreach (var i in this.items)
            {
                writer.WriteLine("[[{0}]]", this.Name);
                i.WriteTo(writer);
                writer.WriteLine();
            }
        }
    }
}
