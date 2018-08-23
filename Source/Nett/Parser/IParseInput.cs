using System;
using System.Collections.Generic;
using System.Linq;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal interface ICommentsContext : IDisposable
    {
        IEnumerable<Comment> Consume();
    }

    internal interface IParseInput
    {
        bool IsFinished { get; }

        Token Current { get; }

        Token Advance();

        bool Peek(Func<Token, bool> predicate);

        ICommentsContext CreateConsumeCommentContext();
    }

    internal static class ParseInputExtensions
    {
        public static IProduction1 Accept(this IParseInput input, Func<Token, bool> predicate)
        {
            IProduction production = new Production(input);
            return production.Accept(predicate);
        }

        public static IProduction1 Expect(this IParseInput input, Func<Token, bool> predicate)
        {
            IProduction production = new Production(input);
            return production.Expect(predicate);
        }

        public static bool AcceptNewLines(this IParseInput input)
        {
            int accpted = 0;
            while (input.Peek(t => t.Type == TokenType.NewLine))
            {
                ++accpted;
                input.Advance();
            }

            return accpted > 0;
        }
    }

    internal sealed class NoCommentsHere : ICommentsContext
    {
        public static readonly ICommentsContext Instance = new NoCommentsHere();

        public IEnumerable<Comment> Consume()
            => Enumerable.Empty<Comment>();

        public void Dispose()
        {
            // Dummy implementation; nothing to do here
        }
    }
}
