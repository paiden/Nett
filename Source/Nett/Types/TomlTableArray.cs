namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Nett.Writer;

    public sealed class TomlTableArray : TomlObject
    {
        private static readonly Type ListType = typeof(IList);
        private static readonly Type ObjectType = typeof(object);
        private static readonly Type TableArrayType = typeof(TomlTableArray);

        private readonly List<TomlTable> items = new List<TomlTable>();

        public TomlTableArray(ITomlRoot root, IEnumerable<TomlTable> enumerable)
            : base(root)
        {
            if (enumerable != null)
            {
                foreach (var e in enumerable)
                {
                    this.Add(e);
                }
            }
        }

        internal TomlTableArray(ITomlRoot root)
            : this(root, null)
        {
        }

        public int Count => this.items.Count;

        public List<TomlTable> Items => this.items;

        public override string ReadableTypeName => "array of tables";

        public override TomlObjectType TomlType => TomlObjectType.ArrayOfTables;

        public TomlTable this[int index] => this.items[index];

        public void Add(TomlTable table)
        {
            this.items.Add(table);
        }

        public override string ToString()
            => TomlTableWriter.WriteTomlFragment(this);

        public TomlTable Last() => this.items[this.items.Count - 1];

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneArrayFor(root);

        internal TomlTableArray CloneArrayFor(ITomlRoot root)
            => CopyComments(new TomlTableArray(root, this.items.Select(i => i.CloneTableFor(root))), this);

        internal override object GetInternal(Type t, Func<IEnumerable<string>> getMyKeyChain)
        {
            if (t == TableArrayType) { return this; }

            var conv = this.Root.Settings.TryGetConverter(this.GetType(), t);
            if (conv != null)
            {
                return conv.Convert(this.Root, this, t);
            }

            if (t.IsArray)
            {
                var et = t.GetElementType();
                var a = Array.CreateInstance(et, this.items.Count);
                int cnt = 0;
                foreach (var i in this.items)
                {
                    a.SetValue(i.GetInternal(et, getMyKeyChain), cnt++);
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
                collection.Add(i.GetInternal(itemType, getMyKeyChain));
            }

            return collection;
        }
    }
}
