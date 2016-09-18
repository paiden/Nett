namespace Nett.Coma
{
    using System.Collections.Generic;
    using System.Diagnostics;
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

    internal interface IMergedSourceFactory
    {
        IMergeableConfig CreateMergedPersistable();
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

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    internal sealed class FileConfigSource : IConfigSource, IMergedSourceFactory, ISourceFactory
    {
        public FileConfigSource(string filePath)
            : this(filePath, filePath)
        {
        }

        public FileConfigSource(string filePath, string alias)
        {
            this.Alias = alias.CheckNotNull(nameof(alias));
            this.FilePath = filePath.CheckNotNull(nameof(alias));
        }

        public string FilePath { get; }

        public string Alias { get; }

        public IPersistableConfig CreatePersistable() => new OptimizedFileConfig(new FileConfig(this));

        public IMergeableConfig CreateMergedPersistable()
            => new MergedConfig(new List<IPersistableConfig>() { this.CreatePersistable() });

        private string DebuggerDisplay => $"[FileSource] Alias={this.Alias} FilePath={this.FilePath}";
    }

    internal sealed class MergeSource : IConfigSource, IMergedSourceFactory
    {
        private IConfigSource[] sources;

        public MergeSource(IConfigSource[] sources)
        {
            this.sources = sources.CheckNotNull(nameof(sources));
        }

        public string Alias => "Aggregate";

        public IMergeableConfig CreateMergedPersistable()
        {
            var sourceFactories = this.sources.Cast<ISourceFactory>();
            var sourcePersistables = sourceFactories.Select(sf => sf.CreatePersistable());
            return new MergedConfig(sourcePersistables);
        }
    }
}
