namespace Nett.Coma
{
    public interface IConfigStore
    {
        bool EnsureExists(TomlTable content);

        TomlTable Load();

        void Save(TomlTable content);

        bool WasChangedExternally();
    }

    internal interface IConfigStoreWithSource : IConfigStore, IConfigSource
    {
        bool CanHandleSource(IConfigSource source);

        TomlTable LoadSourcesTable();
    }
}
