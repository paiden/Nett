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
                var props = settings.GetSerializationProperties(t);

                foreach (var p in props)
                {
                    object val = p.GetValue(obj, null);
                    if (val != null)
                    {
                        TomlObject to = TomlObject.CreateFrom(tt, val, p);
                        AddComments(to, p);
                        tt.AddRow(settings.GetPropertyKey(p), to);
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
        }
    }
}
