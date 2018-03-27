using System;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal sealed class UnacceptableProduction : IProduction1, IProduction2, IProduction3
    {
        public UnacceptableProduction()
        {
        }

        public IProduction3 Accept(Func<Token, bool> predicate)
            => this;

        public T Create<T>(Func<Token, T> onSuccess, Func<SyntaxErrorNode> onError = null)
            => default(T);

        public Node CreateNode(Func<Token, Token, Node> onSuccess, Func<SyntaxErrorNode> onError = null)
                => null;

        public IProduction3 Expect(Func<Token, bool> predicate)
            => this;

        IProduction2 IProduction1.Accept(Func<Token, bool> predicate)
            => this;

        IReq<T> IProduction1.CreateNode<T>(Func<Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(null);

        IOpt<T> IProduction1.CreateNode<T>(Func<Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> onError)
            => new Opt<T>(null);

        IReq<T> IProduction2.CreateNode<T>(Func<Token, Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(null);

        IOpt<T> IProduction2.CreateNode<T>(Func<Token, Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> onError)
            => new Opt<T>(null);

        IReq<T> IProduction3.CreateNode<T>(Func<Token, Token, Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(null);

        IOpt<T> IProduction3.CreateNode<T>(Func<Token, Token, Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> onError)
            => new Opt<T>(null);

        IProduction2 IProduction1.Expect(Func<Token, bool> predicate)
            => this;
    }
}
