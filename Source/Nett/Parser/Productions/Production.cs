namespace Nett.Parser.Productions
{
    internal abstract class Production<T>
    {
        public abstract T Apply(LookaheadBuffer<Token> tokens);
    }
}
