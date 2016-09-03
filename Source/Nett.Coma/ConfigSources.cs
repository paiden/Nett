namespace Nett.Coma
{
    using System.Linq;
    using Nett.Extensions;

    public interface IConfigSource
    {
        string Alias { get; }
    }

    internal interface ISourceFactory
    {
        IPersistableConfig CreatePersistable();
    }

    public static class ConfigSource
    {
        public static IConfigSource CreateFileSource(string filePath)
            => CreateFileSource(filePath, filePath);

        public static IConfigSource CreateFileSource(string filePath, string alias)
            => new FileConfigSource(filePath, alias);

        public static IConfigSource Merged(params IConfigSource[] sources)
            => new MergeSource(sources);
    }

    internal sealed class FileConfigSource : IConfigSource, ISourceFactory
    {
        private readonly string filePath;

        public FileConfigSource(string filePath, string alias)
        {
            this.Alias = alias.CheckNotNull(nameof(alias));
            this.filePath = filePath.CheckNotNull(nameof(alias));
        }

        public string Alias { get; }

        public IPersistableConfig CreatePersistable()
            => new OptimizedFileConfig(new FileConfig(this.filePath));
    }

    internal sealed class MergeSource : IConfigSource, ISourceFactory
    {
        private IConfigSource[] sources;

        public MergeSource(IConfigSource[] sources)
        {
            this.sources = sources.CheckNotNull(nameof(sources));
        }

        public string Alias => "Aggregate";

        public IPersistableConfig CreatePersistable()
        {
            var sourceFactories = this.sources.Cast<ISourceFactory>();
            var sourcePersistables = sourceFactories.Select(sf => sf.CreatePersistable());
            return new MergedConfig(sourcePersistables);
        }

    }
}
