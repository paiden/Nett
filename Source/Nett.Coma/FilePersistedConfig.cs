namespace Nett.Coma
{
    using System.IO;

    internal sealed class FileConfig : IPersistableConfig
    {
        private readonly string filePath;

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

        public TomlTable Load() => Toml.ReadFile(this.filePath);

        public void Save(TomlTable config)
        {
            Toml.WriteFile(config, this.filePath);
        }
    }
}
