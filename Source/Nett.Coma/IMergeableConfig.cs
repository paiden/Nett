namespace Nett.Coma
{
    internal interface IMergeableConfig : IPersistableConfig
    {
        TomlTable Load(IConfigSource source);

        void Save(TomlTable table, IConfigSource target);
    }
}
