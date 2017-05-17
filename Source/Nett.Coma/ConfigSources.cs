namespace Nett.Coma
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Nett.Extensions;

    public interface IConfigSource
    {
        string Alias { get; }

        IConfigSource PrimarySource { get; }
    }

    internal interface IMergedSourceFactory
    {
        IMergeableConfig CreateMergedPersistable();
    }

    internal interface ISourceFactory
    {
        IConfigStore CreatePersistable();
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

        public string Alias { get; }

        public string FilePath { get; }

        public IConfigSource PrimarySource => this;

        private string DebuggerDisplay => $"[FileSource] Alias={this.Alias} FilePath={this.FilePath}";

        public IMergeableConfig CreateMergedPersistable()
            => new MergedConfig(new List<IConfigStore>() { this.CreatePersistable() });

        public IConfigStore CreatePersistable() => new ReloadOnExternalChangeFileConfig(new FileConfigStore(this));
    }

    internal sealed class MergeSource : IConfigSource, IMergedSourceFactory
    {
        private IConfigSource[] sources;

        public MergeSource(IConfigSource[] sources)
        {
            this.sources = sources.CheckNotNull(nameof(sources));
        }

        public string Alias => "Aggregate";

        public IConfigSource PrimarySource => this.sources[this.sources.Length - 1];

        public IMergeableConfig CreateMergedPersistable()
        {
            var sourceFactories = this.sources.Cast<ISourceFactory>();
            var sourcePersistables = sourceFactories.Select(sf => sf.CreatePersistable());
            return new MergedConfig(sourcePersistables);
        }
    }
}
