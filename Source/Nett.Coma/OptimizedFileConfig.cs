namespace Nett.Coma
{
    using Nett.Extensions;

    internal sealed class OptimizedFileConfig : IPersistableConfig
    {
        private readonly FileConfig persistable;

        private TomlTable loaded = null;

        public OptimizedFileConfig(FileConfig persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));
        }

        public bool EnsureExists(TomlTable content) => this.persistable.EnsureExists(content);

        public TomlTable Load()
        {
            if (this.loaded == null || this.persistable.WasChangedExternally())
            {
                this.loaded = this.persistable.Load();
            }

            return this.loaded;
        }

        public void Save(TomlTable content)
        {
            this.persistable.Save(content);
        }

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();
    }
}
