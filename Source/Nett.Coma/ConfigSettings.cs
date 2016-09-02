namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ConfigSettings
    {
        internal static readonly ConfigSettings DefaultInstance = new ConfigSettings();

        public interface IConfigSettingsBuilder
        {
            IConfigSettingsBuilder EnableLoadSaveOptimizations(bool enable);
        }

        private bool LoadSaveOptmizationsEnabled { get; set; } = true;

        public static ConfigSettings Create(Action<IConfigSettingsBuilder> applySettings)
        {
            var config = new ConfigSettings();
            applySettings(new ConfigSettingsBuilder(config));
            return config;
        }

        internal IPersistableConfig GetPersistableConfig(string filePath)
        {
            var config = new FileConfig(filePath);

            if (!this.LoadSaveOptmizationsEnabled)
            {
                return config;
            }

            return new OptimizedFileConfig(config);
        }

        internal IEnumerable<IPersistableConfig> GetPersistableConfig(string[] filePathes)
        {
            var configs = filePathes.Select(fp => new FileConfig(fp));

            if (!this.LoadSaveOptmizationsEnabled)
            {
                return configs;
            }

            return configs.Select(c => new OptimizedFileConfig(c));
        }

        private sealed class ConfigSettingsBuilder : IConfigSettingsBuilder
        {
            private readonly ConfigSettings settings;

            public ConfigSettingsBuilder(ConfigSettings settings)
            {
                this.settings = settings;
            }

            public IConfigSettingsBuilder EnableLoadSaveOptimizations(bool enable)
            {
                this.settings.LoadSaveOptmizationsEnabled = enable;
                return this;
            }
        }
    }
}
