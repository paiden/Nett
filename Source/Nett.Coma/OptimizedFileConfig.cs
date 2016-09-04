namespace Nett.Coma
{
    using Nett.Extensions;

    internal sealed class OptimizedFileConfig : IPersistableConfig
    {
        private readonly FileConfig persistable;

        private TomlTable loaded = null;
        private TomlTable loadedSourcesTable = null;

        public OptimizedFileConfig(FileConfig persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));
        }

        public bool EnsureExists(TomlTable content) => this.persistable.EnsureExists(content);

        public TomlTable Load()
        {
            this.EnsureLastestTableLoaded();
            return this.loaded;
        }

        public TomlTable LoadSourcesTable()
        {
            this.EnsureLastestTableLoaded();
            return this.loadedSourcesTable;
        }

        public void Save(TomlTable content)
        {
            this.persistable.Save(content);
        }

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void EnsureLastestTableLoaded()
        {
            if (this.loaded == null || this.persistable.WasChangedExternally())
            {
                this.loaded = this.persistable.Load();
                this.loadedSourcesTable = this.persistable.LoadSourcesTable();
            }
        }
    }
}
