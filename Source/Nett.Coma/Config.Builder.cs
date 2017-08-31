using System;
using System.Collections.Generic;
using Nett.Extensions;

namespace Nett.Coma
{
    public sealed partial class Config
    {
        public interface IConfigBuilder
        {
            IConfigBuilder<T> MappedToType<T>(Func<T> createDefault)
                where T : class;

            IConfigBuilder StoredAs(Action<IStoreBuilder> storeBuilder);

            IConfigBuilder UseTomlConfiguration(TomlSettings config);

            Config Initialize();
        }

        public interface IConfigBuilder<T>
            where T : class
        {
            IConfigBuilder<T> StoredAs(Action<IStoreBuilder> storeBuilder);

            IConfigBuilder<T> UseTomlConfiguration(TomlSettings config);

            Config<T> Initialize();
        }

        public interface IStoreBuilder
        {
            IMergeStoreBuilder File(string filePath);
        }

        public interface IMergeStoreBuilder
        {
            IMergeStoreBuilder AsSourceWithName(string name);

            IStoreBuilder MergeWith();
        }

        public static IConfigBuilder CreateAs()
        {
            return new ConfigBuilder();
        }

        private class StoreBuilder : IStoreBuilder, IMergeStoreBuilder
        {
            private readonly List<SourceInfo> sourceInfos = new List<SourceInfo>();

            IMergeStoreBuilder IMergeStoreBuilder.AsSourceWithName(string name)
            {
                var item = this.sourceInfos[this.sourceInfos.Count - 1];
                this.sourceInfos[this.sourceInfos.Count - 1] = new SourceInfo()
                {
                    StoreFactory = item.StoreFactory,
                    Name = name,
                };

                return this;
            }

            public IMergeConfigStore CreateStore(TomlSettings config, TomlTable content)
            {
                List<IConfigStoreWithSource> stores = new List<IConfigStoreWithSource>(this.sourceInfos.Count);

                foreach (var i in this.sourceInfos)
                {
                    var store = i.StoreFactory(i, config);
                    stores.Add(store);
                }

                var mergeStore = new MergeConfigStore(stores);
                mergeStore.EnsureExists(content);

                return mergeStore;
            }

            IMergeStoreBuilder IStoreBuilder.File(string filePath)
            {
                this.sourceInfos.Add(new SourceInfo()
                {
                    StoreFactory = (srcInfo, cfg) => new FileConfigStore(cfg, filePath, srcInfo.Name),
                    Name = filePath,
                });

                return this;
            }

            IStoreBuilder IMergeStoreBuilder.MergeWith()
            {
                return this;
            }

            private struct SourceInfo
            {
                public Func<SourceInfo, TomlSettings, IConfigStoreWithSource> StoreFactory;
                public string Name;
            }
        }

        private class ConfigBuilder : IConfigBuilder
        {
            protected TomlSettings config = TomlSettings.DefaultInstance;
            protected StoreBuilder storeBuilder = new StoreBuilder();

            Config IConfigBuilder.Initialize()
            {
                return new Config(this.storeBuilder.CreateStore(this.config, Toml.Create()));
            }

            IConfigBuilder<T> IConfigBuilder.MappedToType<T>(Func<T> createDefault)
            {
                return new ConfigBuilder<T>(createDefault)
                {
                    config = this.config,
                };
            }

            IConfigBuilder IConfigBuilder.StoredAs(Action<IStoreBuilder> build)
            {
                build(this.storeBuilder);
                return this;
            }

            IConfigBuilder IConfigBuilder.UseTomlConfiguration(TomlSettings config)
            {
                this.config = config.CheckNotNull(nameof(config));

                return this;
            }
        }

        private class ConfigBuilder<T> : ConfigBuilder, IConfigBuilder<T>
            where T : class
        {
            private readonly Func<T> createDefault;

            public ConfigBuilder(Func<T> createDefault)
            {
                this.createDefault = createDefault.CheckNotNull(nameof(createDefault));
            }

            Config<T> IConfigBuilder<T>.Initialize()
            {
                var initTable = Toml.Create(this.createDefault(), this.config);
                return new Config<T>(this.storeBuilder.CreateStore(this.config, initTable));
            }

            IConfigBuilder<T> IConfigBuilder<T>.StoredAs(Action<IStoreBuilder> storeBuilder)
            {
                storeBuilder(this.storeBuilder);
                return this;
            }

            IConfigBuilder<T> IConfigBuilder<T>.UseTomlConfiguration(TomlSettings config)
            {
                this.config = config.CheckNotNull(nameof(config));

                return this;
            }
        }
    }
}
