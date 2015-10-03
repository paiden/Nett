namespace Nett.Parser.Productions
{
    internal abstract class GenerativeProduction<T>
    {
        public abstract T Apply(LookaheadBuffer<Token> tokens);
    }
}
