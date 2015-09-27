using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            var p = new TomlTableProduction();
            return (TomlTable)p.Apply(this.Tokens);
        }
    }
}
