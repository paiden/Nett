namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using static System.Diagnostics.Debug;

    internal class MergedConfig : IPersistableConfig
    {
        private const string AssertAtLeastOneConfig =
            "Constructor should check that there is a config and the configs should not get modified later on";

        private readonly IEnumerable<IPersistableConfig> configs;

        public MergedConfig(IEnumerable<IPersistableConfig> configs)
        {
            if (configs == null) { throw new ArgumentNullException(nameof(configs)); }
            if (configs.Count() <= 0) { throw new ArgumentException("There needs to be at least one config", nameof(configs)); }

            this.configs = configs;
        }

        public bool EnsureExists(TomlTable content)
        {
            Assert(this.configs.Count() > 0, AssertAtLeastOneConfig);

            return this.configs.First().EnsureExists(content);
        }

        public TomlTable Load()
        {
            Assert(this.configs.Count() > 0, AssertAtLeastOneConfig);

            TomlTable merged = this.configs.First().Load();

            foreach (var c in this.configs.Skip(1))
            {
                merged.OverwriteWithValuesFrom(c.Load());
            }

            return merged;
        }

        public void Save(TomlTable content)
        {
            throw new NotImplementedException();
        }
    }
}
