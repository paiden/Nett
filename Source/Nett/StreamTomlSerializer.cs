namespace Nett
{
    using System.IO;
    using Nett.Parser;
    using Nett.Parser.Builders;

    public static class StreamTomlSerializer
    {
        public static TomlTable Deserialize(Stream s, TomlSettings settings)
        {
            var reader = new StreamReader(s);
            var content = reader.ReadToEnd();
            var lexer = new Lexer(content);
            var tokens = lexer.Lex();
            var parser = new Parser.Parser(new ParseInput(tokens), settings);
            var ast = parser.Parse();
#if DEBUG
            var tree = ast.SyntaxNodeOrDefault()?.PrintTree();
#endif
            var table = TableBuilder.Build(ast.SyntaxNodeOrDefault(), settings);
            return table;
        }
    }
}
