namespace Nett
{
    using System;
    using System.Collections;

    public partial class TomlTable
    {
        internal static RootTable From<T>(TomlSettings settings, T obj)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            if ((object)obj is RootTable rt) { return rt; }

            var tt = new RootTable(settings);

            if ((object)obj is IDictionary dict)
            {
                CreateFromDictionary();
            }
            else
            {
                CreateFromCustomClrObject();
            }

            return tt;

            void CreateFromDictionary()
            {
                foreach (DictionaryEntry r in dict)
                {
                    tt.AddRow(new TomlKey((string)r.Key), TomlObject.CreateFrom(tt, r.Value));
                }
            }

            void CreateFromCustomClrObject()
            {
                var t = obj.GetType();
                var members = settings.GetSerializationMembers(t);

                foreach (var m in members)
                {
                    object val = m.GetValue(obj);
                    if (val != null)
                    {
                        TomlObject to = TomlObject.CreateFrom(tt, val);
                        to.AddComments(settings.GetComments(t, m.Member));
                        tt.AddRow(m.Key, to);
                    }
                }
            }
        }

        internal sealed class RootTable : TomlTable, ITomlRoot
        {
            private readonly TomlSettings settings;

            public RootTable(TomlSettings settings)
                : base(null)
            {
                if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

                this.settings = settings;
            }

            TomlSettings ITomlRoot.Settings => this.settings;

            internal override TomlObject CloneFor(ITomlRoot root)
            {
                var clone = root == this ? new RootTable(this.settings) : new TomlTable(root);
                this.CloneForInternal(clone);
                return clone;
            }
        }
    }
}
