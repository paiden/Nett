namespace Nett.Coma
{
    /// <summary>
    /// Interface for a configuration store
    /// </summary>
    public interface IConfigStore
    {
        /// <summary>
        /// Method called by the TOML library to initialize the store.
        /// </summary>
        /// <remarks>
        /// If the stored configuration (e.g. a file) already exists, the method should do nothing and return <b>false</b> to
        /// tell the calling component the store exists already. Otherwise the implementation should create the store
        /// (e.g. write a new TOML file) and return <b>true</b>.
        /// </remarks>
        /// <param name="content">The TOML content that should get stored.</param>
        /// <returns><b>true</b> if a new store was created, <b>false</b> if the store already existed.</returns>
        bool EnsureExists(TomlTable content);

        /// <summary>
        /// Loads and returns a TOML table from the store.
        /// </summary>
        /// <returns>The loaded TOML table.</returns>
        TomlTable Load();

        /// <summary>
        /// Saves a TOML table in the store.
        /// </summary>
        /// <param name="content">The TOML table that should get stored.</param>
        void Save(TomlTable content);

        /// <summary>
        /// Method that checks if the content was modified externally. E.g. a file was modified by a user on disk.
        /// </summary>
        /// <remarks>
        /// This method should not return true, if the store was modified by the implementation itself. If the store
        /// implementation cannot determine if external modifications happened, always returning <b>true</b> is valid,
        /// but may degrade performance significantly.
        /// </remarks>
        /// <returns><b>true</b> if the store was modified, <b>false</b> otherwise.</returns>
        bool WasChangedExternally();
    }

    internal interface IConfigStoreWithSource : IConfigStore
    {
        IConfigSource Source { get; }

        bool CanHandleSource(IConfigSource source);

        TomlTable LoadSourcesTable();
    }
}
