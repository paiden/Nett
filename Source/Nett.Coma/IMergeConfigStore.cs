using System.Collections.Generic;

namespace Nett.Coma
{
    internal interface IMergeConfigStore : IConfigStoreWithSource
    {
        IEnumerable<IConfigSource> Sources { get; }

        TomlTable Load(IConfigSource source);

        void Save(TomlTable table, IConfigSource target);
    }
}
