using System;
using Nett.Extensions;

namespace Nett.Parser.Nodes
{
    internal interface IParsed<out T>
    {
        bool HasNode { get; }

        Node NodeOrDefault();
    }

    internal interface IOpt<out T> : IParsed<T>
        where T : Node
    {
        T SyntaxNodeOrDefault();

        IReq<T> AsReq();

        IOpt<TRes> As<TRes>()
            where TRes : Node;
    }

    internal interface IReq<out T> : IParsed<T>
        where T : Node
    {
        Node Node();

        T SyntaxNode();

        IOpt<T> AsOpt();
    }

    internal static class AstNode
    {
        public static IOpt<T> Optional<T>(T node)
            where T : Node
            => new Opt<T>(node);

        public static IOpt<T> None<T>()
            where T : Node
            => new Opt<T>(null);

        public static IReq<T> Required<T>(T node)
            where T : Node
            => new Req<T>(node);

        public static IOpt<T> Opt<T>(this T node)
            where T : Node
            => Optional(node);

        public static IReq<T> Req<T>(this T node)
            where T : Node
            => Required(node);

        public static IOpt<T> Or<T>(this IOpt<T> x, Func<IOpt<T>> y)
            where T : Node
            => x.HasNode ? x : y();

        public static IReq<T> Orr<T>(this IOpt<T> x, Func<IReq<T>> y)
            where T : Node
            => x.HasNode ? x.AsReq() : y();

        public static IReq<T> OrNode<T>(this IOpt<T> x, Func<Node> y)
            where T : Node
            => x.HasNode ? x.AsReq() : new Req<T>(y());
    }

    internal sealed class Req<T> : Opt<T>, IReq<T>
        where T : Node
    {
        public Req(Node node)
            : base(node)
        {
            node.CheckNotNull(nameof(node));
        }

        public Node Node()
            => this.NodeOrDefault();

        public T SyntaxNode()
            => this.SyntaxNodeOrDefault();

        IOpt<T> IReq<T>.AsOpt()
            => this;
    }

    internal class Opt<T> : IOpt<T>
      where T : Node
    {
        public static readonly Opt<T> None = new Opt<T>(null);

        private readonly T syntaxNode;
        private readonly SyntaxErrorNode errorNode;

        public Opt(Node node)
        {
            if (node is SyntaxErrorNode se) { this.errorNode = se; }
            else { this.syntaxNode = (T)node; }
        }

        public bool HasNode
            => this.syntaxNode != null || this.errorNode != null;

        public IOpt<TRes> As<TRes>()
            where TRes : Node
        {
            return new Opt<TRes>((Node)this.syntaxNode ?? this.errorNode);
        }

        public IReq<T> AsReq()
        {
            if (!this.HasNode) { throw new InvalidOperationException("Opt without node cannot be converted to Req."); }
            return new Req<T>((Node)this.syntaxNode ?? this.errorNode);
        }

        public Node NodeOrDefault()
            => this.syntaxNode ?? this.errorNode ?? default(Node);

        public T SyntaxNodeOrDefault()
        {
            if (this.errorNode != null) { throw ParseException.FromSyntaxError(this.errorNode); }

            return this.syntaxNode ?? default(T);
        }
    }
}
