namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using static System.Diagnostics.Debug;

    internal class MergedConfig : IPersistableConfig
    {
        private const string AssertAtLeastOneConfigMsg =
            "Constructor should check that there is a config and the configs should not get modified later on";

        private readonly List<IPersistableConfig> configs;

        public MergedConfig(IEnumerable<IPersistableConfig> configs)
        {
            if (configs == null) { throw new ArgumentNullException(nameof(configs)); }
            if (configs.Count() <= 0) { throw new ArgumentException("There needs to be at least one config", nameof(configs)); }

            this.configs = new List<IPersistableConfig>(configs);
        }

        public bool EnsureExists(TomlTable content)
        {
            Assert(this.configs.Count > 0, AssertAtLeastOneConfigMsg);

            return this.configs.First().EnsureExists(content);
        }

        public TomlTable Load()
        {
            Assert(this.configs.Count > 0, AssertAtLeastOneConfigMsg);

            TomlTable merged = this.configs.First().Load();
            foreach (var c in this.configs.Skip(1))
            {
                merged.OverwriteWithValuesForLoadFrom(c.Load());
            }

            return merged;
        }

        public void Save(TomlTable content)
        {
            Assert(this.configs.Count > 0, AssertAtLeastOneConfigMsg);

            foreach (var c in this.configs)
            {
                var tbl = c.Load();
                tbl.OverwriteWithValuesForSaveFrom(content);
                c.Save(tbl);
            }
        }
    }
}
