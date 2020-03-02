using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private static readonly TPath Root = new TPath(null, new PathSegment(typeof(object)));

        private readonly TPath parent;
        private readonly PathSegment segment;

        private TPath(TPath parent, PathSegment segment)
        {
            this.parent = parent;
            this.segment = segment;
        }

        public static TPath Build(LambdaExpression expression)
        {
            return BuildInternal(expression.Body);
        }

        public bool Clear(TomlObject target)
        {
            var tgt = this.parent == null ? target : this.parent.TryGet(target);
            if (tgt != null)
            {
                return this.segment.Clear(tgt);
            }

            return false;
        }

        public void Set(TomlObject target, Func<TomlObject, TomlObject> createNewValueObject)
        {
            var tgt = this.parent?.Resolve(target) ?? target;
            this.segment.Set(tgt, createNewValueObject);
        }

        public TomlObject Get(TomlObject target)
        {
            var tgt = this.parent?.Get(target) ?? target;
            return this.segment.Get(tgt);
        }

        public TomlObject TryGet(TomlObject target)
        {
            try
            {
                return this.Get(target);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
            => $"{this.parent?.ToString() ?? string.Empty}{this.segment.ToString()}";

        internal IEnumerable<TPath> BuildForTableItems(TomlTable tbl)
        {
            foreach (var r in tbl.InternalRows)
            {
                yield return new TPath(this, new RowSegment(typeof(object), r.Key.Value));
            }
        }

        private static TomlObject GetTableRowOrThrowOnNotFound(string key, TomlTable tbl)
            => tbl.TryGetValue(key, out var val) ? val : throw new KeyNotFoundException($"Key '{key}' not found in TOML table.");

        private static TomlObject GetTableRowOrCreateDefault(string key, TomlTable tbl, Type rowType)
        {
            return tbl.TryGetValue(key, out var val) ? val : CreateDefault();

            TomlObject CreateDefault()
            {
                return rowType switch
                {
                    _ => tbl[key] = tbl.CreateEmptyAttachedTable(),
                };
            }
        }

        private static TPath BuildInternal(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression me:
                    var path = BuildInternal(me.Expression);
                    return new TPath(path, GetSegmentFromMemberExpression(me));
                case BinaryExpression be:
                    path = BuildInternal(be.Left);
                    var seg = GetSegmentFromConstantExpression((ConstantExpression)be.Right);
                    return new TPath(path, seg);
                case ParameterExpression pe:
                    return Root;
                case MethodCallExpression mce:
                    path = BuildInternal(mce.Object);
                    seg = GetSegmentFromConstantExpression((ConstantExpression)mce.Arguments.Single());
                    return new TPath(path, seg);
                default:
                    throw new InvalidOperationException($"TPath cannot be created as expression '{expression.GetType()}' cannot be handled.");
            }
        }

        private static PathSegment GetSegmentFromConstantExpression(ConstantExpression ce)
            => ce switch
            {
                _ when ce.Value is int i => new IndexSegment(ce.Type, i),
                _ when ce.Value is long l => new IndexSegment(ce.Type, (int)l),
                _ when ce.Value is string s => new RowSegment(ce.Type, s),
                _ => throw new InvalidOperationException($"Cannot convert constant expression '{ce}' to TPath segment."),
            };

        private static PathSegment GetSegmentFromMemberExpression(MemberExpression expr)
        {
            return new RowSegment(expr.Type, expr.Member.Name);
        }

        private TomlObject Resolve(TomlObject target)
        {
            var tgt = this.parent?.Resolve(target) ?? target;
            return this.segment.Resolve(tgt, ResolveParent);

            TomlObject ResolveParent()
                => this.parent?.parent?.Resolve(target) ?? tgt;
        }

        private class PathSegment
        {
            protected readonly Type mappedType;

            public PathSegment(Type mappedType)
            {
                this.mappedType = mappedType;
            }

            public virtual void Set(TomlObject target, Func<TomlObject, TomlObject> createNewValueObject)
                => throw new NotSupportedException();

            public virtual TomlObject Get(TomlObject target)
                => target;

            public virtual TomlObject Resolve(TomlObject target, Func<TomlObject> resolveTargetsParent)
                => target;

            public virtual bool Clear(TomlObject target)
                => throw new NotSupportedException();

            public override string ToString() => string.Empty;
        }

        private sealed class IndexSegment : PathSegment
        {
            private readonly int index;

            public IndexSegment(Type segmentType, int index)
                : base(segmentType)
            {
                this.index = index;
            }

            public override TomlObject Get(TomlObject target)
                => target switch
                {
                    TomlArray ta => ta[this.index],
                    TomlTableArray tta => tta[this.index],
                    _ => throw new InvalidOperationException("X1"),
                };

            public override TomlObject Resolve(TomlObject target, Func<TomlObject> resolveTargetsParent)
                => this.Get(target);

            public override void Set(TomlObject target, Func<TomlObject, TomlObject> createNewValueObject)
            {
                if (target is TomlTableArray tta)
                {
                    tta.Items[this.index] = (TomlTable)createNewValueObject(tta.Items[this.index]);
                }
                else if (target is TomlArray ta)
                {
                    ta.Items[this.index] = (TomlValue)createNewValueObject(ta.Items[this.index]);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot apply index '[{this.index}]' onto TOML object of type '{target.ReadableTypeName}'.");
                }
            }

            public override string ToString()
                => $"[{this.index}]";
        }
    }
}
