namespace Nett
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class TomlTable
    {
        internal static RootTable From<T>(TomlConfig config, T obj)
        {
            if (config == null) { throw new ArgumentNullException(nameof(config)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            var rt = obj as RootTable;
            if (rt != null) { return rt; }

            var tt = new RootTable(config);
            var t = obj.GetType();
            var props = config.GetSerializationProperties(t);
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

            tt.AddScopeTypesLast(allObjects);

            return tt;
        }

        internal sealed class RootTable : TomlTable, ITomlRoot
        {
            private readonly TomlConfig config;

            public RootTable(TomlConfig config)
                : base(null)
            {
                if (config == null) { throw new ArgumentNullException(nameof(config)); }

                this.config = config;
            }

            TomlConfig ITomlRoot.Config => this.config;
        }
    }
}
