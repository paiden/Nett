using System;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal interface IProduction
    {
        IProduction1 Accept(Func<Token, bool> predicate);

        IProduction1 Expect(Func<Token, bool> predicate);
    }

    internal interface IProduction1
    {
        IReq<T> CreateNode<T>(Func<Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> onError = null)
            where T : Node;

        IOpt<T> CreateNode<T>(Func<Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> onError = null)
            where T : Node;

        IProduction2 Accept(Func<Token, bool> predicate);

        IProduction2 Expect(Func<Token, bool> predicate);
    }

    internal interface IProduction2
    {
        IReq<T> CreateNode<T>(Func<Token, Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> onError = null)
            where T : Node;

        IOpt<T> CreateNode<T>(Func<Token, Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> onError = null)
            where T : Node;

        IProduction3 Accept(Func<Token, bool> predicate);

        IProduction3 Expect(Func<Token, bool> predicate);
    }

    internal interface IProduction3
    {
        IOpt<T> CreateNode<T>(Func<Token, Token, Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> onError = null)
            where T : Node;

        IReq<T> CreateNode<T>(Func<Token, Token, Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> onError = null)
            where T : Node;
    }
}
