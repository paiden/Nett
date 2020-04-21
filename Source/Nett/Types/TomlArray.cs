namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Nett.Writer;

    public sealed class TomlArray : TomlValue<TomlValue[]>
    {
        internal TomlArray(ITomlRoot root, params TomlValue[] values)
            : base(root, values)
        {
            if (values.Length > 1)
            {
                TomlObjectType primaryType = values[0].TomlType;

                for (int i = 1; i < values.Length; i++)
                {
                    if (values[i].TomlType != primaryType)
                    {
                        throw new ArgumentException($"All TOML array elements should have the same type as the first item. " +
                            $"First item has type '{values[0].ReadableTypeName}', but element at index '{i}' has type " +
                            $"'{values[i].ReadableTypeName}'.");
                    }
                }
            }
        }

        public TomlValue[] Items => this.Value;

        public int Length => this.Value.Length;

        public override string ReadableTypeName => "array";

        public override TomlObjectType TomlType => TomlObjectType.Array;

        internal override string Unit
        {
            get => null;
            set => throw new InvalidOperationException($"{this.ReadableTypeName} cannot have a unit.");
        }

        public TomlObject this[int index] => this.Value[index];

        public T Get<T>(int index) => this.Value[index].Get<T>();

        public TomlObject Last() => this.Value[this.Value.Length - 1];

        public IEnumerable<T> To<T>() => this.To<T>(TomlSettings.DefaultInstance);

        public IEnumerable<T> To<T>(TomlSettings settings) => this.Value.Select((to) => to.Get<T>());

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
            => TomlTableWriter.WriteTomlFragment(this);

        internal TomlArray CloneArrayFor(ITomlRoot root)
        {
            TomlValue[] copy = new TomlValue[this.Value.Length];
            this.Value.CopyTo(copy, 0);
            return CopyComments(new TomlArray(root, copy), this);
        }

        internal override object GetInternal(Type t, Func<IEnumerable<string>> _)
        {
            if (t == Types.TomlArrayType) { return this; }

            if (t.IsArray)
            {
                Type et = t.GetElementType();
                Array a = Array.CreateInstance(et, this.Value.Length);
                int cnt = 0;
                foreach (TomlValue i in this.Value)
                {
                    a.SetValue(i.Get(et), cnt++);
                }

                return a;
            }

            if (Types.ListType.IsAssignableFrom(t))
            {
                IList collection = (IList)Activator.CreateInstance(t);
                Type itemType = Types.ObjectType;
                if (t.IsGenericType)
                {
                    itemType = t.GetGenericArguments()[0];
                }

                foreach (TomlValue i in this.Value)
                {
                    collection.Add(i.Get(itemType));
                }

                return collection;
            }

            var conv = this.Root.Settings.TryGetConverter(Types.TomlArrayType, t);
            if (conv != null)
            {
                return conv.Convert(this.Root, this, t);
            }

            throw new InvalidOperationException(string.Format("Cannot convert TOML array to '{0}'.", t.FullName));
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneArrayFor(root);
    }
}
