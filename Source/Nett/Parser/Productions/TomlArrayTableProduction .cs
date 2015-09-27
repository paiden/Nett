using System;
using System.Diagnostics;

namespace Nett.Parser.Productions
{
    internal sealed class TomlArrayTableProduction : Production<TomlTableArray>
    {
        public override TomlTableArray Apply(LookaheadBuffer<Token> tokens)
        {

            //var t0 = this.Consume();
            //var t1 = this.Consume();

            //Debug.Assert(t0.type == TokenType.LBrac && t1.type == TokenType.LBrac);

            return null;
        }
    }
}
