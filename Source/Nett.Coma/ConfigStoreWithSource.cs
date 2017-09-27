namespace Nett.Coma
{
    using Nett.Extensions;

    /// <summary>
    /// Combines source &amp; store and optimized loading mechanisms.
    /// </summary>
    internal sealed class ConfigStoreWithSource : IConfigStoreWithSource
    {
        private readonly IConfigStore store;

        private TomlTable loaded = null;
        private TomlTable loadedSourcesTable = null;

        public ConfigStoreWithSource(IConfigSource source, IConfigStore persistable)
        {
            this.Source = source.CheckNotNull(nameof(source));
            this.store = persistable.CheckNotNull(nameof(persistable));
        }

        public IConfigSource Source { get; }

        public string Name { get; }

        public bool CanHandleSource(IConfigSource source) => this.Source.Name == source.Name;

        public bool EnsureExists(TomlTable content) => this.store.EnsureExists(content);

        public TomlTable Load()
        {
            this.EnsureLastestTableLoaded();
            return this.loaded.Clone();
        }

        public TomlTable LoadSourcesTable()
        {
            this.EnsureLastestTableLoaded();
            return this.loadedSourcesTable.Clone();
        }

        public void Save(TomlTable content)
        {
            this.store.Save(content);
            this.loaded = content;
            this.loadedSourcesTable = this.Load().TransformToSourceTable(this.Source);
            this.loadedSourcesTable.Freeze();
        }

        public bool WasChangedExternally() => this.store.WasChangedExternally();

        private void EnsureLastestTableLoaded()
        {
            if (this.loaded == null || this.store.WasChangedExternally())
            {
                this.loaded = this.store.Load();
                this.loadedSourcesTable = this.loaded.TransformToSourceTable(this.Source);

                this.loaded.Freeze();
                this.loadedSourcesTable.Freeze();
            }
        }
    }
}
