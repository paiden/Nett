using System;
using System.IO;
using Nett.Parser.Productions;
using static System.Diagnostics.Debug;

namespace Nett.Parser
{
    internal sealed class Parser
    {
        private readonly Tokenizer tokenizer;
        private TokenBuffer Tokens => this.tokenizer.Tokens;
        private readonly TomlConfig config;

        public Parser(Stream s, TomlConfig config)
        {
            Assert(config != null);

            this.tokenizer = new Tokenizer(s);
            this.config = config;
        }

        public TomlTable Parse()
        {
            return this.Toml();
        }

        private TomlTable Toml()
        {
            var root = new TomlTable.RootTable(this.config) { IsDefined = true };
            TomlTable current = root;

            while (!Tokens.End)
            {
                current = ExpressionsProduction.TryApply(current, root, this.Tokens);
                if (current == null)
                {
                    if (!Tokens.End)
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
