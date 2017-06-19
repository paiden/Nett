namespace Nett.Parser
{
    using System;
    using System.IO;
    using Nett.Parser.Productions;
    using static System.Diagnostics.Debug;

    internal sealed class Parser
    {
        private readonly TomlSettings settings;
        private readonly Tokenizer tokenizer;

        public Parser(Stream s, TomlSettings settings)
        {
            Assert(settings != null);

            this.tokenizer = new Tokenizer(s);
            this.settings = settings;
        }

        private TokenBuffer Tokens => this.tokenizer.Tokens;

        public static Exception CreateParseError(FilePosition pos, string message)
            => new Exception($"Line {pos.Line}, Column {pos.Column}: {message}");

        public static Exception CreateParseError(Token position, string message)
            => CreateParseError(new FilePosition { Line = position.line, Column = position.col }, message);

        public TomlTable Parse()
        {
            return this.Toml();
        }

        private TomlTable Toml()
        {
            var root = new TomlTable.RootTable(this.settings) { IsDefined = true };
            TomlTable current = root;

            while (!this.Tokens.End)
            {
                current = ExpressionsProduction.TryApply(current, root, this.Tokens);
                if (current == null)
                {
                    if (!this.Tokens.End)
                    {
                        throw new Exception();
                    }

                    break;
                }
            }

            return root;
        }
    }
}
