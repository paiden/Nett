namespace Nett.Coma
{
    internal interface IPersistableConfig
    {
        bool EnsureExists(TomlTable content);

        TomlTable Load();

        TomlTable LoadSourcesTable();

        void Save(TomlTable content);

        bool WasChangedExternally();
    }
}
