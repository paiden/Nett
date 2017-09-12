using System.IO;
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

        public static IConfigurationBuilder AddTomlFile(
            this IConfigurationBuilder builder,
            IFileProvider provider,
            string path,
            bool optional,
            bool reloadOnChange)
        {
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }

            var source = new TomlConfigurationSource()
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
            };

            builder.Add(source);
            return builder;
        }
    }
}
