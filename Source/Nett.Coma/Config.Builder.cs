using System;
using System.Collections.Generic;
using Nett.Extensions;

namespace Nett.Coma
{
    public sealed partial class Config
    {
        /// <summary>
        /// Builder object to configure a untyped 'Coma' configuration.
        /// </summary>
        public interface IConfigBuilder
        {
            /// <summary>
            /// Specifies the CLR type to that the configuration object should get mapped.
            /// </summary>
            /// <typeparam name="T">Type of the CLR object</typeparam>
            /// <param name="createDefault">Func used to create a config object with it's defaults.</param>
            /// <returns>A builder object that allows to setup the config system further.</returns>
            IConfigBuilder<T> MappedToType<T>(Func<T> createDefault)
                where T : class;

            /// <summary>
            /// Used to define how the config should be persisted.
            /// </summary>
            /// <param name="storeBuilder">Action used to configure the store.</param>
            /// <returns>A builder object that allows to setup the config system further.</returns>
            IConfigBuilder StoredAs(Action<IStoreBuilder> storeBuilder);

            /// <summary>
            /// Sets the TOML settings this config should use when reading &amp; serializing and deserializing its
            /// contents to/from TOML.
            /// </summary>
            /// <param name="config">The settings to use.</param>
            /// <returns>A builder object that allows to setup the config system further.</returns>
            /// <exception cref="ArgumentNullException">If <i>config</i> is <b>null</b>.</exception>"
            IConfigBuilder UseTomlConfiguration(TomlSettings config);

            /// <summary>
            /// Finalizes the configuration and creates a new config object with the specified settings.
            /// </summary>
            /// <returns>The newly created config object.</returns>
            Config Initialize();
        }

        /// <summary>
        /// Builder object to configure a strongly typed 'Coma' configuration.
        /// </summary>
        /// <typeparam name="T">The type of the CLR object the config should get mapped to.</typeparam>
        public interface IConfigBuilder<T>
            where T : class
        {
            /// <summary>
            /// Used to define how the config should be persisted.
            /// </summary>
            /// <param name="storeBuilder">Action used to configure the store.</param>
            /// <returns>A builder object that allows to setup the config system further.</returns>
            IConfigBuilder<T> StoredAs(Action<IStoreBuilder> storeBuilder);

            /// <summary>
            /// Sets the TOML settings this config should use when reading &amp; serializing and deserializing its
            /// contents to/from TOML.
            /// </summary>
            /// <param name="config">The settings to use.</param>
            /// <returns>A builder object that allows to setup the config system further.</returns>
            /// <exception cref="ArgumentNullException">If <i>config</i> is <b>null</b>.</exception>"
            IConfigBuilder<T> UseTomlConfiguration(TomlSettings config);

            /// <summary>
            /// Finalizes the configuration and creates a new config object with the specified settings.
            /// </summary>
            /// <returns>The newly created config object.</returns>
            Config<T> Initialize();
        }

        /// <summary>
        /// Builder interface to specify a config store.
        /// </summary>
        public interface IStoreBuilder
        {
            /// <summary>
            /// Store the config in this file.
            /// </summary>
            /// <param name="filePath">An absolute or relative path to the file where the config should be stored.</param>
            /// <returns>A builder object allowing to specify additional stores.</returns>
            IMergeStoreBuilder File(string filePath);

            /// <summary>
            /// Use a custom store implementation for this configuration.
            /// </summary>
            /// <param name="store">The store to use for the configuration.</param>
            /// <returns>A builder object allowing to specify additional stores.</returns>
            /// <exception cref="ArgumentNullException">if <i>store</i> is <b>null</b>.</exception>
            IMergeStoreBuilder CustomStore(IConfigStore store);
        }

        /// <summary>
        /// Builder interface allowing to build merged config stores (config separated into multiple sources e.g. files)
        /// </summary>
        public interface IMergeStoreBuilder
        {
            /// <summary>
            /// Give the previously defined store a name and retrieve it.
            /// </summary>
            /// <param name="name">The name the store should have.</param>
            /// <param name="src">When this method returns, contains the source object with the given name.</param>
            /// <returns>A builder object allowing to specify additional stores.</returns>
            IMergeStoreBuilder AccessedBySource(string name, out IConfigSource src);

            /// <summary>
            /// Merge previously defined store with another store to build up the config. This store will override the config
            /// values of the previous store.
            /// </summary>
            /// <param name="builder">Builder object used to create the new store.</param>
            /// <returns>A Builder object that allows to create stores.</returns>
            IStoreBuilder MergeWith(IStoreBuilder builder);

            /// <summary>
            /// Merge previously defined store with another store to build up the config. This store will override the config
            /// values of the previous store.
            /// </summary>
            /// <param name="builder">Builder object used to create the new store.</param>
            /// <returns>A Builder object that allows to create stores.</returns>
            IStoreBuilder MergeWith(IMergeStoreBuilder builder);
        }

        /// <summary>
        /// Returns a new builder object to configure and create a new 'Coma' configuration object.
        /// </summary>
        /// <returns>The new builder.</returns>
        public static IConfigBuilder CreateAs()
        {
            return new ConfigBuilder();
        }

        private class StoreBuilder : IStoreBuilder, IMergeStoreBuilder
        {
            private readonly List<StoreFactoryInfo> sourceInfos = new List<StoreFactoryInfo>();

            private StoreFactoryInfo LatestFactoryInfo => this.sourceInfos[this.sourceInfos.Count - 1];

            public IMergeStoreBuilder AccessedBySource(string name, out IConfigSource src)
            {
                StoreFactoryInfo latest = this.LatestFactoryInfo;
                src = new ConfigSource(name);
                this.sourceInfos[this.sourceInfos.Count - 1] = new StoreFactoryInfo()
                {
                    StoreFactory = latest.StoreFactory,
                    Source = src,
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

            IMergeStoreBuilder IStoreBuilder.CustomStore(IConfigStore store)
            {
                this.sourceInfos.Add(new StoreFactoryInfo()
                {
                    StoreFactory = (srcInfo, cfg) => new ConfigStoreWithSource(srcInfo.Source, store),
                    Source = new ConfigSource(Guid.NewGuid().ToString()),
                });

                return this;
            }

            IMergeStoreBuilder IStoreBuilder.File(string filePath)
            {
                this.sourceInfos.Add(new StoreFactoryInfo()
                {
                    StoreFactory = (srcInfo, cfg) => new ConfigStoreWithSource(
                        srcInfo.Source, new FileConfigStore(cfg, filePath)),
                    Source = new ConfigSource(filePath),
                });

                return this;
            }

            IStoreBuilder IMergeStoreBuilder.MergeWith(IStoreBuilder builder)
            {
                return this;
            }

            IStoreBuilder IMergeStoreBuilder.MergeWith(IMergeStoreBuilder builder)
            {
                return this;
            }

            private struct StoreFactoryInfo
            {
                public Func<StoreFactoryInfo, TomlSettings, IConfigStoreWithSource> StoreFactory;
                public IConfigSource Source;
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
