namespace Nett.Coma
{
    public interface IConfigSource
    {
        string Alias { get; }
    }

    //public static class ConfigSource
    //{
    //    public static IConfigSource CreateFileSource(string filePath)
    //        => new FileConfigStore(TomlConfig.DefaultInstance, filePath, filePath);

    //    public static IConfigSource CreateFileSource(string filePath, string alias)
    //        => new FileConfigStore(TomlConfig.DefaultInstance, filePath, alias);

    //    public static IConfigSource Merged(params IConfigSource[] sources)
    //        => new MergeConfigStore(sources.Cast<IConfigStore>());
    //}
}
