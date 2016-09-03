namespace Nett.Coma
{
    using System.IO;
    using System.Security.Cryptography;

    internal sealed class FileConfig : IPersistableConfig
    {
        private readonly string filePath;

        private byte[] latestFileHash = null;

        public FileConfig(string filePath)
        {
            this.filePath = filePath;
        }

        public bool EnsureExists(TomlTable content)
        {
            if (!File.Exists(this.filePath))
            {
                this.Save(content);
                return true;
            }

            return false;
        }

        public bool WasChangedExternally()
        {
            var current = ComputeHash(this.filePath);
            return !HashEquals(this.latestFileHash, current);
        }

        public TomlTable Load()
        {
            this.latestFileHash = ComputeHash(this.filePath);
            return Toml.ReadFile(this.filePath);
        }

        public void Save(TomlTable config)
        {
            Toml.WriteFile(config, this.filePath);
            this.latestFileHash = ComputeHash(this.filePath);
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
