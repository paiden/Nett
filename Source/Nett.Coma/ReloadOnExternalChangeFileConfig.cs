namespace Nett.Coma
{
    using Nett.Extensions;

    internal sealed class ReloadOnExternalChangeFileConfig : IConfigStoreWithSource
    {
        private readonly FileConfigStore persistable;

        private TomlTable loaded = null;
        private TomlTable loadedSourcesTable = null;

        public ReloadOnExternalChangeFileConfig(FileConfigStore persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));
        }

        public string Name => this.persistable.Name;

        public bool CanHandleSource(IConfigSource source) => this.persistable.CanHandleSource(source);

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
            this.loadedSourcesTable = this.persistable.TransformToSourceTable(this.loaded);
            this.loadedSourcesTable.Freeze();
        }

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void EnsureLastestTableLoaded()
        {
            if (this.loaded == null || this.persistable.WasChangedExternally())
            {
                this.loaded = this.persistable.Load();
                this.loadedSourcesTable = this.persistable.TransformToSourceTable(this.loaded);

                this.loaded.Freeze();
                this.loadedSourcesTable.Freeze();
            }
        }
    }
}
