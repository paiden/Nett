using System.Collections.Generic;

namespace Nett.Coma
{
    internal interface IMergeConfigStore : IConfigStore
    {
        IEnumerable<IConfigSource> Sources { get; }

        TomlTable Load(IConfigSource source);

        void Save(TomlTable table, IConfigSource target);
    }
}
