namespace Nett.Coma
{
    using System;

    public interface IPersistableConfig : IDisposable
    {
        bool EnsureExists(TomlTable content);

        TomlTable Load();

        void Save(TomlTable content);
    }
}
