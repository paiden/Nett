using System;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal abstract class ProductionBase
    {
        private static readonly UnacceptableProduction CannotAccept = new UnacceptableProduction();

        private readonly IParseInput input;

        protected ProductionBase(IParseInput input)
        {
            this.input = input;
        }

        protected Token Advance()
            => this.input.Advance();

        protected T Apply<T>(Func<Token, bool> predicate, Func<Token, T> onSuccess, Func<T> onFail)
            => this.input.Peek(predicate)
                ? onSuccess(this.Advance())
                : onFail();

        protected ErrorProduction CreateError()
            => new ErrorProduction(this.input.Current);

        protected UnacceptableProduction DoNotAccept()
            => CannotAccept;
    }

    internal sealed class Production : ProductionBase, IProduction, IProduction1, IProduction2, IProduction3
    {
        private Token token1;
        private Token token2;
        private Token token3;

        public Production(IParseInput input)
            : base(input)
        {
        }

        // IProduction
        IProduction1 IProduction.Accept(Func<Token, bool> predicate)
            => this.Apply(
                predicate,
                onSuccess: this.ApplyToken1,
                onFail: this.DoNotAccept);

        IProduction1 IProduction.Expect(Func<Token, bool> predicate)
            => this.Apply(
                predicate,
                onSuccess: this.ApplyToken1,
                onFail: this.CreateError);

        // IProduction1
        IReq<T> IProduction1.CreateNode<T>(Func<Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> _)
            => onSuccess(this.token1);

        IOpt<T> IProduction1.CreateNode<T>(Func<Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> _)
            => onSuccess(this.token1);

        public T Create<T>(Func<Token, T> onSuccess, Func<SyntaxErrorNode> _)
            => onSuccess(this.token1);

        IProduction2 IProduction1.Accept(Func<Token, bool> predicate)
            => this.Apply(
                predicate,
                onSuccess: this.ApplyToken2,
                onFail: this.DoNotAccept);

        IProduction2 IProduction1.Expect(Func<Token, bool> predicate)
            => this.Apply(
                predicate,
                onSuccess: this.ApplyToken2,
                onFail: this.CreateError);

        // IProduction2
        IReq<T> IProduction2.CreateNode<T>(Func<Token, Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> _)
            => onSuccess(this.token1, this.token2);

        IOpt<T> IProduction2.CreateNode<T>(Func<Token, Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> _)
            => onSuccess(this.token1, this.token2);

        IProduction3 IProduction2.Accept(Func<Token, bool> predicate)
            => this.Apply(
                predicate,
                onSuccess: this.ApplyToken3,
                onFail: this.DoNotAccept);

        IProduction3 IProduction2.Expect(Func<Token, bool> predicate)
            => this.Apply(
                predicate,
                onSuccess: this.ApplyToken3,
                onFail: this.CreateError);

        // IProduction3
        IReq<T> IProduction3.CreateNode<T>(Func<Token, Token, Token, IReq<T>> onSuccess, Func<Token, SyntaxErrorNode> _)
            => onSuccess(this.token1, this.token2, this.token3);

        IOpt<T> IProduction3.CreateNode<T>(Func<Token, Token, Token, IOpt<T>> onSuccess, Func<Token, SyntaxErrorNode> _)
            => onSuccess(this.token1, this.token2, this.token3);

        private IProduction1 ApplyToken1(Token t)
        {
            this.token1 = t;
            return this;
        }

        private IProduction2 ApplyToken2(Token t)
        {
            this.token2 = t;
            return this;
        }

        private IProduction3 ApplyToken3(Token t)
        {
            this.token3 = t;
            return this;
        }
    }
}
