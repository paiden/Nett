namespace Nett.Coma
{
    public interface IPersistableConfig
    {
        bool EnsureExists(TomlTable content);

        TomlTable Load();

        void Save(TomlTable content);

        bool WasChangedExternally();
    }
}
