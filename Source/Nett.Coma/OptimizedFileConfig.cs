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
            return this.loaded.Clone();
        }

        public TomlTable LoadSourcesTable()
        {
            this.EnsureLastestTableLoaded();
            return this.loadedSourcesTable.Clone();
        }

        public void Save(TomlTable content)
        {
            this.persistable.Save(content);
            this.loaded = content;
            this.loadedSourcesTable = this.persistable.TransformToSourceTable(this.loaded).Freeze();
        }

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void EnsureLastestTableLoaded()
        {
            if (this.loaded == null || this.persistable.WasChangedExternally())
            {
                this.loaded = this.persistable.Load().Freeze();
                this.loadedSourcesTable = this.persistable.TransformToSourceTable(this.loaded).Freeze();
            }
        }

        public bool CanHandleSource(IConfigSource source) => this.persistable.CanHandleSource(source);
    }
}
