namespace Nett
{
    using System.IO;

    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s, TomlConfig config)
        {
            var parser = new Parser.Parser(s, config);
            return parser.Parse();
        }
    }
}
