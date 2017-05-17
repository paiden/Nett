namespace Nett.Coma
{
    using System.Linq;

    public interface IConfigSource
    {
        string Alias { get; }
    }

    public static class ConfigSource
    {
        public static IConfigSource CreateFileSource(string filePath)
            => new FileConfigStore(filePath, filePath);

        public static IConfigSource CreateFileSource(string filePath, string alias)
            => new FileConfigStore(filePath, alias);

        public static IConfigSource Merged(params IConfigSource[] sources)
            => new MergeConfigStore(sources.Cast<IConfigStore>());
    }
}
