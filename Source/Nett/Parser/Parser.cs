using System;
using System.IO;
using Nett.Parser.Productions;

namespace Nett.Parser
{
    internal sealed class Parser
    {
        private readonly Tokenizer tokenizer;
        private LookaheadBuffer<Token> Tokens => this.tokenizer.Tokens;



        public Parser(Stream s)
        {
            this.tokenizer = new Tokenizer(s);
        }

        public TomlTable Parse()
        {
            return this.Toml();
        }

        private TomlTable Toml()
        {
            TomlTable root = new TomlTable();
            TomlTable current = root;


            while (!Tokens.End)
            {
                var exp = new ExpressionsProduction(current, root);
                current = exp.Apply(this.Tokens);
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
