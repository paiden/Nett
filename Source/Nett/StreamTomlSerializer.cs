using System;
using System.IO;

namespace Nett
{
    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s)
        {
            var scanner = new Parser.Scanner(s);
            var parser = new Parser.Parser(scanner);
            var sw = new StringWriter();
            parser.errors.errorStream = sw;
            parser.Parse();
            if(parser.errors.count > 0)
            {
                throw new Exception("Failed to parse TOML: " + sw.ToString());
            }

            return parser.parsed;
        }
    }
}
