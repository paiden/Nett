namespace Nett
{
    using System;

    public static class Coma
    {
        public static ComaConfig<T> Create<T>(string filePath, Func<T> createDefault)
            where T : class
        {
            var cfg = createDefault();
            var persisted = new FileConfig<T>(filePath);

            persisted.EnsureExists(cfg);

            return new ComaConfig<T>(persisted);
        }
    }
}
