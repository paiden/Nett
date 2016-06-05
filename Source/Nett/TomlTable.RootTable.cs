using System;
using System.Collections.Generic;

namespace Nett
{
    public partial class TomlTable
    {
        internal sealed class RootTable : TomlTable, IMetaDataStore
        {
            private readonly TomlConfig config;

            public RootTable(TomlConfig config)
                : base(null)
            {
                if (config == null) { throw new ArgumentNullException(nameof(config)); }

                this.config = config;
                this.SetAsMetaDataRoot(this);
            }

            TomlConfig IMetaDataStore.Config => this.config;
        }

        internal static RootTable From<T>(TomlConfig config, T obj)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            var rt = obj as RootTable;
            if (rt != null) { return rt; }

            var tt = new RootTable(config);
            var t = obj.GetType();
            var props = t.GetProperties();
            var allObjects = new List<Tuple<string, TomlObject>>();

            foreach (var p in props)
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    TomlObject to = TomlObject.CreateFrom(tt, val, p);
                    AddComments(to, p);
                    allObjects.Add(Tuple.Create(p.Name, to));
                }
            }

            tt.AddValues(allObjects);
            tt.AddComplex(allObjects);

            return tt;
        }
    }
}
