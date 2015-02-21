using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nett
{
    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s)
        {
            var scanner = new Scanner(s);
            var parser = new Parser(scanner);
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
