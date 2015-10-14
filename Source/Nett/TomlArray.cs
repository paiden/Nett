using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nett
{
    public sealed class TomlArray : TomlValue<TomlValue[]>
    {
        public static readonly TomlArray Empty = new TomlArray(new TomlValue[0]);

        private static readonly Type TomlArrayType = typeof(TomlArray);
        private static readonly Type ListType = typeof(IList);
        private static readonly Type ObjectType = typeof(object);

        public override string ReadableTypeName => "array";

        public TomlArray(params TomlValue[] values) : base(values)
        {
        }

        public TomlValue[] Items => this.Value;

        public int Length => this.Value.Length;

        public TomlObject this[int index] => this.Value[index];

        public T Get<T>(int index) => this.Value[index].Get<T>();

        public override object Get(Type t, TomlConfig config)
        {
            if (t == TomlArrayType) { return this; }

            if (t.IsArray)
            {
                var et = t.GetElementType();
                var a = Array.CreateInstance(et, this.Value.Length);
                int cnt = 0;
                foreach (var i in this.Value)
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

            foreach (var i in this.Value)
            {
                collection.Add(i.Get(itemType, config));
            }

            return collection;
        }

        public TomlObject Last() => this.Value[this.Value.Length - 1];

        public IEnumerable<T> To<T>() => this.To<T>(TomlConfig.DefaultInstance);
        public IEnumerable<T> To<T>(TomlConfig config) => this.Value.Select((to) => to.Get<T>(config));

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
