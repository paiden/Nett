namespace Nett
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class TomlTable
    {
        internal static RootTable From<T>(TomlSettings settings, T obj)
        {
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            var rt = obj as RootTable;
            if (rt != null) { return rt; }

            var tt = new RootTable(settings);
            var t = obj.GetType();
            var props = settings.GetSerializationProperties(t);
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
