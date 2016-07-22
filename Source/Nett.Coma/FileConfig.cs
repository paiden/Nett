namespace Nett.Coma
{
    using System.IO;

    internal sealed class FileConfig<T> : IPersistedConfig<T>
    {
        private readonly string filePath;

        public FileConfig(string filePath)
        {
            this.filePath = filePath;
        }

        public bool EnsureExists(T def)
        {
            if (!File.Exists(this.filePath))
            {
                this.Save(def);
                return true;
            }

            return false;
        }

        public T Load() => Toml.ReadFile<T>(this.filePath);

        public void Save(T config)
        {
            Toml.WriteFile(config, this.filePath);
        }
    }
}
