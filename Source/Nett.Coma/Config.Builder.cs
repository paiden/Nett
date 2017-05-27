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

            IConfigBuilder UseTomlConfiguration(TomlConfig config);

            Config Initialize();
        }

        public interface IConfigBuilder<T>
            where T : class
        {
            IConfigBuilder<T> StoredAs(Action<IStoreBuilder> storeBuilder);

            IConfigBuilder<T> UseTomlConfiguration(TomlConfig config);

            Config<T> Initialize();
        }

        public interface IStoreBuilder
        {
            IMergeStoreBuilder File(string filePath);
        }

        public interface IMergeStoreBuilder
        {
            IMergeStoreBuilder AsSource(Action<IConfigSource> sourceCreatedCallback);

            IStoreBuilder MergeWith();
        }

        public static IConfigBuilder CreateAs()
        {
            return new ConfigBuilder();
        }

        private class StoreBuilder : IStoreBuilder, IMergeStoreBuilder
        {
            private readonly List<Tuple<string, string, Action<IConfigSource>>> items
                = new List<Tuple<string, string, Action<IConfigSource>>>();

            IMergeStoreBuilder IMergeStoreBuilder.AsSource(Action<IConfigSource> sourceCreatedCallback)
            {
                var item = this.items[this.items.Count - 1];
                this.items[this.items.Count - 1] =
                    new Tuple<string, string, Action<IConfigSource>>(item.Item1, item.Item2, sourceCreatedCallback);

                return this;
            }

            public IMergeConfigStore CreateStore(TomlConfig config, TomlTable content)
            {
                List<IConfigStore> stores = new List<IConfigStore>(this.items.Count);

                foreach (var i in this.items)
                {
                    var store = new FileConfigStore(config, i.Item1, i.Item2);
                    i.Item3(store);
                    stores.Add(store);
                }

                var mergeStore = new MergeConfigStore(stores);
                mergeStore.EnsureExists(content);

                return mergeStore;
            }

            IMergeStoreBuilder IStoreBuilder.File(string filePath)
            {
                this.items.Add(new Tuple<string, string, Action<IConfigSource>>(filePath, filePath, _ => { }));
                return this;
            }

            IStoreBuilder IMergeStoreBuilder.MergeWith()
            {
                return this;
            }
        }

        private class ConfigBuilder : IConfigBuilder
        {
            protected TomlConfig config = TomlConfig.DefaultInstance;
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

            IConfigBuilder IConfigBuilder.UseTomlConfiguration(TomlConfig config)
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

            IConfigBuilder<T> IConfigBuilder<T>.UseTomlConfiguration(TomlConfig config)
            {
                this.config = config;

                return this;
            }
        }
    }
}
