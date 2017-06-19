namespace Nett
{
    using System.IO;

    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s, TomlSettings settings)
        {
            var parser = new Parser.Parser(s, settings);
            return parser.Parse();
        }
    }
}
