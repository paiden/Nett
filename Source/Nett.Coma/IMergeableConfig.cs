namespace Nett.Coma
{
    internal interface IMergeableConfig : IConfigStore
    {
        TomlTable Load(IConfigSource source);

        void Save(TomlTable table, IConfigSource target);
    }
}
