namespace Nett.Coma
{
    internal interface IConfigStore : IConfigSource
    {
        //string Alias { get; }

        bool CanHandleSource(IConfigSource source);

        bool EnsureExists(TomlTable content);

        TomlTable Load();

        TomlTable LoadSourcesTable();

        void Save(TomlTable content);

        bool WasChangedExternally();
    }
}
