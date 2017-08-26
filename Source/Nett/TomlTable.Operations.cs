using System;
using System.Collections.Generic;
using System.Linq;
using Nett.Extensions;

namespace Nett
{
    using CommentsOp = Func<InputComments, IEnumerable<TomlComment>>;

    public interface ITableCombiner
    {
    }

    public interface ITargetSelector
    {
        ISourceSelector Overwrite(TomlTable target);
    }

    public interface ISourceSelector
    {
        ICommentOperationOrRowSelector With(TomlTable source);
    }

    public interface IRowSelector
    {
        ITableCombiner ForAllSourceRows();

        ITableCombiner ForAllTargetRows();

        ITableCombiner ForRowsOnlyInSource();
    }

    public interface ICommentOperationOrRowSelector : IRowSelector
    {
        IRowSelector IncludingAllComments(bool append = false);

        IRowSelector IncludingNewComments();

        IRowSelector ExcludingComments();

        IRowSelector IncludingComments(CommentsOp combiner);
    }

    internal interface ICommentsOperation
    {
        IEnumerable<TomlComment> Combine(IEnumerable<TomlComment> target, IEnumerable<TomlComment> source);
    }

    internal interface ITableOperation : ITableCombiner
    {
        TomlTable Execute();
    }

    public struct InputComments
    {
        internal InputComments(IEnumerable<TomlComment> tgt, IEnumerable<TomlComment> src)
        {
            this.TargetComments = tgt;
            this.SourceComments = src;
        }

        public IEnumerable<TomlComment> TargetComments { get; internal set; }

        public IEnumerable<TomlComment> SourceComments { get; internal set; }
    }

    public partial class TomlTable
    {
        internal sealed class TableOperationBuilder : ITargetSelector
        {
            public TableOperationBuilder()
            {
            }

            ISourceSelector ITargetSelector.Overwrite(TomlTable target)
            {
                return new OverwriteTableOperationBuilder(target);
            }
        }

        internal sealed class OverwriteTableOperationBuilder : ISourceSelector, ICommentOperationOrRowSelector
        {
            private static readonly CommentsOp IncludeAllCommentsAndReplace;
            private static readonly CommentsOp IncludeAllCommentsAndAppend;
            private static readonly CommentsOp IncludeNewComments;
            private static readonly CommentsOp ExcludeComments;

            private readonly TomlTable target;
            private TomlTable source;
            private Func<InputComments, IEnumerable<TomlComment>> commentsOp = IncludeAllCommentsAndReplace;

            static OverwriteTableOperationBuilder()
            {
                IncludeAllCommentsAndReplace = i => i.SourceComments;
                IncludeAllCommentsAndAppend = i => i.TargetComments.Union(i.SourceComments);
                IncludeNewComments = i => i.TargetComments.Count() <= 0 ? i.SourceComments : i.TargetComments;
                ExcludeComments = i => i.TargetComments;
            }

            public OverwriteTableOperationBuilder(TomlTable target)
            {
                this.target = target;
            }

            ITableCombiner IRowSelector.ForAllSourceRows()
                => new OverwriteAllSourceRowsOperation(this.target, this.source, this.commentsOp);

            ITableCombiner IRowSelector.ForRowsOnlyInSource()
                => new OverwriteSourceOnlyRowsOperation(this.target, this.source, this.commentsOp);

            ITableCombiner IRowSelector.ForAllTargetRows()
                => new OverwriteAllTargetRowsOperation(this.target, this.source, this.commentsOp);

            ICommentOperationOrRowSelector ISourceSelector.With(TomlTable source)
            {
                this.source = source;
                return this;
            }

            IRowSelector ICommentOperationOrRowSelector.IncludingAllComments(bool append)
            {
                this.commentsOp = append ? IncludeAllCommentsAndAppend : IncludeAllCommentsAndReplace;
                return this;
            }

            IRowSelector ICommentOperationOrRowSelector.ExcludingComments()
            {
                this.commentsOp = ExcludeComments;
                return this;
            }

            IRowSelector ICommentOperationOrRowSelector.IncludingNewComments()
            {
                this.commentsOp = IncludeNewComments;
                return this;
            }

            IRowSelector ICommentOperationOrRowSelector.IncludingComments(CommentsOp combiner)
            {
                this.commentsOp = combiner.CheckNotNull(nameof(combiner));
                return this;
            }
        }

        internal sealed class IncludeAllCommentsOperation : ICommentsOperation
        {
            IEnumerable<TomlComment> ICommentsOperation.Combine(IEnumerable<TomlComment> target, IEnumerable<TomlComment> source)
            {
                return source;
            }
        }

        internal sealed class ExcludeCommentsOperation : ICommentsOperation
        {
            IEnumerable<TomlComment> ICommentsOperation.Combine(IEnumerable<TomlComment> target, IEnumerable<TomlComment> source)
            {
                return target;
            }
        }

        internal abstract class TableCombineOperation : ITableOperation
        {
            protected readonly TomlTable target;
            protected readonly TomlTable source;
            protected CommentsOp commentsOp;

            private static readonly IEnumerable<TomlComment> Empty = new List<TomlComment>();

            public TableCombineOperation(
                TomlTable target, TomlTable source, CommentsOp commentsOp)
            {
                this.target = target.CheckNotNull(nameof(target));
                this.source = source.CheckNotNull(nameof(source));
                this.commentsOp = commentsOp.CheckNotNull(nameof(commentsOp));
            }

            public virtual TomlTable Execute() => this.target.CloneTableFor(this.target.Root);

            protected IEnumerable<TomlComment> GetComments(TomlKey k)
            {
                var tgtComments = this.target.TryGetValue(k.Value, out var tgtVal) ? tgtVal.Comments : Empty;
                var srcComments = this.source.TryGetValue(k.Value, out var srcVal) ? srcVal.Comments : Empty;
                return this.commentsOp(new InputComments(tgtComments, srcComments));
            }

            protected TomlObject CloneWithComments(TomlKey key, TomlObject sourceValue, ITomlRoot root)
            {
                var comments = this.GetComments(key);
                var cloned = sourceValue.CloneFor(root);
                cloned.ClearComments();
                cloned.AddComments(comments.ToList());
                return cloned;
            }
        }

        internal sealed class OverwriteAllSourceRowsOperation : TableCombineOperation
        {
            public OverwriteAllSourceRowsOperation(
                TomlTable target, TomlTable source, CommentsOp commentsOp)
                : base(target, source, commentsOp)
            {
            }

            public sealed override TomlTable Execute()
            {
                var combineResult = base.Execute();

                foreach (var r in this.source.rows)
                {
                    combineResult.rows[r.Key] = this.CloneWithComments(r.Key, r.Value, combineResult.Root);
                }

                return combineResult;
            }
        }

        internal sealed class OverwriteAllTargetRowsOperation : TableCombineOperation
        {
            public OverwriteAllTargetRowsOperation(TomlTable target, TomlTable source, CommentsOp commentsOp)
                : base(target, source, commentsOp)
            {
            }

            public sealed override TomlTable Execute()
            {
                var combineResult = base.Execute();

                foreach (var targetRow in this.target.rows)
                {
                    if (this.source.rows.TryGetValue(targetRow.Key, out var sourceValue))
                    {
                        combineResult.rows[targetRow.Key] = this.CloneWithComments(
                            targetRow.Key, sourceValue, combineResult.Root);
                    }
                }

                return combineResult;
            }
        }

        internal sealed class OverwriteSourceOnlyRowsOperation : TableCombineOperation
        {
            public OverwriteSourceOnlyRowsOperation(TomlTable target, TomlTable source, CommentsOp commentsOp)
                : base(target, source, commentsOp)
            {
            }

            public sealed override TomlTable Execute()
            {
                var combineResult = base.Execute();

                foreach (var sourceRow in this.source.rows)
                {
                    if (!combineResult.rows.ContainsKey(sourceRow.Key))
                    {
                        combineResult.rows[sourceRow.Key] = this.CloneWithComments(
                            sourceRow.Key, sourceRow.Value, combineResult.Root);
                    }
                }

                return combineResult;
            }
        }
    }
}
