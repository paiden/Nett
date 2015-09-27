namespace Nett.Parser.Productions
{
    internal sealed class KeyProduction : Production<string>
    {
        public override string Apply(LookaheadBuffer<Token> tokens)
        {
            if (tokens.Expect(TokenType.BareKey)) { return tokens.Consume().value; }
            else if (tokens.Expect(TokenType.String)) { return tokens.Consume().value.Replace("\"", ""); }
            else
            {
                throw new System.Exception($"Failed to parse key because unexpected token '{tokens.Peek().value}' was found.");
            }
        }
    }
}
