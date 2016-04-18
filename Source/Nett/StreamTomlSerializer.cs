using System.IO;

namespace Nett
{
    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s, TomlConfig config)
        {
            var parser = new Parser.Parser(s, config);
            return parser.Parse();
        }
    }
}
