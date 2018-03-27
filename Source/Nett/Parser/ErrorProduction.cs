using System;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal sealed class ErrorProduction : IProduction1, IProduction2, IProduction3
    {
        private readonly Token errorToken;

        public ErrorProduction(Token errorToken)
        {
            this.errorToken = errorToken;
        }

        IProduction2 IProduction1.Accept(Func<Token, bool> predicate)
            => this;

        IProduction3 IProduction2.Accept(Func<Token, bool> predicate)
            => this;

        IReq<T> IProduction1.CreateNode<T>(Func<Token, IReq<T>> _, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(this.GetError(onError));

        IOpt<T> IProduction1.CreateNode<T>(Func<Token, IOpt<T>> _, Func<Token, SyntaxErrorNode> onError)
            => new Opt<T>(this.GetError(onError));

        IReq<T> IProduction2.CreateNode<T>(Func<Token, Token, IReq<T>> _, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(this.GetError(onError));

        IOpt<T> IProduction2.CreateNode<T>(Func<Token, Token, IOpt<T>> _, Func<Token, SyntaxErrorNode> onError)
            => new Opt<T>(this.GetError(onError));

        IReq<T> IProduction3.CreateNode<T>(Func<Token, Token, Token, IReq<T>> _, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(this.GetError(onError));

        IOpt<T> IProduction3.CreateNode<T>(Func<Token, Token, Token, IOpt<T>> _, Func<Token, SyntaxErrorNode> onError)
            => new Req<T>(this.GetError(onError));

        IProduction2 IProduction1.Expect(Func<Token, bool> predicate)
            => this;

        IProduction3 IProduction2.Expect(Func<Token, bool> predicate)
            => this;

        private SyntaxErrorNode GetError(Func<Token, SyntaxErrorNode> customHandler)
            => customHandler?.Invoke(this.errorToken) ?? SyntaxErrorNode.Unexpected(this.errorToken);
    }
}
