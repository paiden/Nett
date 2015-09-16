using System.IO;

namespace Nett
{
    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s)
        {
            var parser = new Parser.Parser(s);
            return parser.Parse();
        }
    }
}
