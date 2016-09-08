namespace Nett.Coma
{
    using System.IO;
    using System.Security.Cryptography;
    using Nett.Extensions;

    internal sealed class FileConfig : IPersistableConfig
    {
        private readonly FileConfigSource source;

        private byte[] latestFileHash = null;

        public FileConfig(FileConfigSource source)
        {
            this.source = source.CheckNotNull(nameof(source));
        }

        private string FilePath => this.source.FilePath;

        public bool CanHandleSource(IConfigSource source) => this.source.Alias == source.Alias;

        public bool EnsureExists(TomlTable content)
        {
            if (!File.Exists(this.source.FilePath))
            {
                this.Save(content);
                return true;
            }

            return false;
        }

        public bool WasChangedExternally()
        {
            var current = ComputeHash(this.FilePath);
            return !HashEquals(this.latestFileHash, current);
        }

        public TomlTable Load()
        {
            this.latestFileHash = ComputeHash(this.FilePath);
            return Toml.ReadFile(this.FilePath);
        }

        public TomlTable LoadSourcesTable()
        {
            var table = this.Load();
            var sourcesTable = table.TransformToSourceTable(this.source);
            return sourcesTable;
        }

        public void Save(TomlTable config)
        {
            Toml.WriteFile(config, this.FilePath);
            this.latestFileHash = ComputeHash(this.FilePath);
        }

        private static byte[] ComputeHash(string filePath)
        {
            var hash = SHA1.Create();
            using (var fileStream = File.OpenRead(filePath))
            {
                return hash.ComputeHash(fileStream);
            }
        }

        private static bool HashEquals(byte[] x, byte[] y)
        {
            if (x == y) { return true; }
            if (x == null || y == null) { return false; }
            if (x.Length != y.Length) { return false; }

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
