namespace Nett.Coma
{
    internal interface IMergeableConfig : IPersistableConfig
    {
        TomlTable Load(TomlTable table, IConfigSource source);

        void Save(TomlTable table, IConfigSource source);
    }
}
