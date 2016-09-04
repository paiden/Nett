namespace Nett.Coma
{
    public interface IPersistableConfig
    {
        bool EnsureExists(TomlTable content);

        TomlTable Load();

        TomlTable LoadSourcesTable();

        void Save(TomlTable content);

        bool WasChangedExternally();
    }
}
