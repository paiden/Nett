using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Nett.AspNet
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path)
            => AddTomlFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);

        public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, bool optional)
            => AddTomlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);

        public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
            => AddTomlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);

        public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange)
            => AddTomlFile(builder, provider, path, s => { s.Optional = optional; s.ReloadOnChange = reloadOnChange; });

        public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, string path, Action<TomlConfigurationSource> configureSource)
            => AddTomlFile(builder, provider: null, path: path, configureSource: configureSource);

        public static IConfigurationBuilder AddTomlFile(this IConfigurationBuilder builder, IFileProvider provider, string path, Action<TomlConfigurationSource> configureSource)
        {
            var source = new TomlConfigurationSource()
            {
                FileProvider = provider,
                Path = path
            };

            configureSource?.Invoke(source);

            source.ResolveFileProvider();

            return builder.Add(source);
        }
    }
}
