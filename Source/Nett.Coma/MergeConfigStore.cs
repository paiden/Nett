namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using static System.Diagnostics.Debug;

    internal class MergeConfigStore : IMergeConfigStore
    {
        private const string AssertAtLeastOneConfigMsg =
            "Constructor should check that there is a config and the configs should not get modified later on";

        private readonly List<IConfigStore> stores;

        public MergeConfigStore(IEnumerable<IConfigStore> configs)
        {
            if (configs == null) { throw new ArgumentNullException(nameof(configs)); }
            if (configs.Count() <= 0) { throw new ArgumentException("There needs to be at least one config", nameof(configs)); }

            this.stores = new List<IConfigStore>(configs);
        }

        public IEnumerable<IConfigSource> Sources => this.stores;

        public string Name => throw new NotImplementedException();

        public bool CanHandleSource(IConfigSource source) => this.stores.Any(c => c.CanHandleSource(source));

        public bool EnsureExists(TomlTable content)
        {
            Assert(this.stores.Count > 0, AssertAtLeastOneConfigMsg);

            return this.stores.First().EnsureExists(content);
        }

        public TomlTable Load() => this.MergeTables(c => c.Load());

        public TomlTable Load(IConfigSource source)
        {
#pragma warning disable SA1312
            IConfigStore _;
            return this.LoadInternal(source, out _);
#pragma warning restore SA1312
        }

        public TomlTable LoadSourcesTable() => this.MergeTables(c => c.LoadSourcesTable());

        public void Save(TomlTable content)
        {
            Assert(this.stores.Count > 0, AssertAtLeastOneConfigMsg);

            foreach (var c in this.stores)
            {
                var tbl = c.Load();
                tbl.OverwriteWithValuesForSaveFrom(content, addNewRows: false);
                c.Save(tbl);
            }
        }

        public void Save(TomlTable table, IConfigSource source)
        {
            IConfigStore cfg = this.stores.Single(c => c.CanHandleSource(source));
            cfg.Save(table);
        }

        public bool WasChangedExternally() => this.stores.Any(c => c.WasChangedExternally());

        private TomlTable LoadInternal(IConfigSource source, out IConfigStore cfg)
        {
            cfg = this.stores.Single(c => c.CanHandleSource(source));
            return cfg.Load();
        }

        private TomlTable MergeTables(Func<IConfigStore, TomlTable> loadSingle)
        {
            Assert(this.stores.Count > 0, AssertAtLeastOneConfigMsg);

            TomlTable merged = Toml.Create();
            foreach (var c in this.stores)
            {
                merged.OverwriteWithValuesForLoadFrom(loadSingle(c));
            }

            return merged;
        }
    }
}
