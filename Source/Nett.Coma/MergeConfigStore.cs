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

        private readonly List<IConfigStoreWithSource> stores;

        public MergeConfigStore(IEnumerable<IConfigStoreWithSource> configs)
        {
            if (configs == null) { throw new ArgumentNullException(nameof(configs)); }
            if (configs.Count() <= 0) { throw new ArgumentException("There needs to be at least one config", nameof(configs)); }

            this.stores = new List<IConfigStoreWithSource>(configs);
        }

        public IConfigSource Source => this.RootSource;

        public IEnumerable<IConfigSource> Sources => this.stores.Select(s => s.Source);

        public IConfigSource RootSource => this.stores.First().Source;

        public string Name => throw new NotImplementedException();

        public bool CanHandleSource(IConfigSource source) => this.stores.Any(c => c.CanHandleSource(source));

        public bool EnsureExists(TomlTable content)
        {
            Assert(this.stores.Count > 0, AssertAtLeastOneConfigMsg);

            // Create broadest scope table
            bool neededToCreate = false;
            neededToCreate |= this.stores.First().EnsureExists(content);

            // Initialize empty files for all other existing scopes
            foreach (var s in this.stores.Skip(1))
            {
                neededToCreate |= s.EnsureExists(Toml.Create());
            }

            return neededToCreate;
        }

        public TomlTable Load() => this.MergeTables(c => c.Load());

        public TomlTable Load(IConfigSource source)
        {
#pragma warning disable SA1312
            IConfigStoreWithSource _;
            return this.LoadInternal(source, out _);
#pragma warning restore SA1312
        }

        public TomlTable LoadSourcesTable() => this.MergeTables(c => c.LoadSourcesTable());

        public void RemoveEmptyTables()
        {
            foreach (var store in this.stores)
            {
                var tbl = store.Load();
                RemoveEmptyTables(tbl);
                store.Save(tbl);
            }

            static void RemoveEmptyTables(TomlTable current)
            {
                foreach (var r in current.InternalRows.ToList())
                {
                    if (r.Value is TomlTable tbl)
                    {
                        if (tbl.InternalRows.Any())
                        {
                            RemoveEmptyTables(tbl);
                        }
                        else
                        {
                            current.Remove(r.Key.Value);
                        }
                    }
                }
            }
        }

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
            IConfigStoreWithSource cfg = this.stores.Single(c => c.CanHandleSource(source));
            cfg.Save(table);
        }

        public bool WasChangedExternally() => this.stores.Any(c => c.WasChangedExternally());

        private TomlTable LoadInternal(IConfigSource source, out IConfigStoreWithSource cfg)
        {
            cfg = this.stores.Single(c => c.CanHandleSource(source));
            return cfg.Load();
        }

        private TomlTable MergeTables(Func<IConfigStoreWithSource, TomlTable> loadSingle)
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
