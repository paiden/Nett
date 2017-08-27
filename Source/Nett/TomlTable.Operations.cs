using System;
using System.Collections.Generic;
using System.Linq;
using Nett.Extensions;

namespace Nett
{
    using CommentsOp = Func<InputComments, IEnumerable<TomlComment>>;

    /// <summary>
    /// Interface representing a built TOML table combine operation.
    /// </summary>
    /// <remarks>
    /// Use <see cref="TomlTable.Combine(Func{ITargetSelector, ITableCombiner})"/> to create and consume combine operations.
    /// </remarks>
    public interface ITableCombiner
    {
    }

    /// <summary>
    /// Builder interface to select the target table for a table combine operation.
    /// </summary>
    public interface ITargetSelector
    {
        /// <summary>
        /// Selects the target table of a table combine operation.
        /// </summary>
        /// <param name="target">The target table. Will not get modified.</param>
        /// <returns>A builder object allowing to select the source table for the combine operation.</returns>
        /// <exception cref="ArgumentNullException">If target is <b>null</b>.</exception>
        ISourceSelector Overwrite(TomlTable target);
    }

    /// <summary>
    /// Builder interface to select the source table for a table combine operation.
    /// </summary>
    public interface ISourceSelector
    {
        /// <summary>
        /// Selects the source table for a table combine operation.
        /// </summary>
        /// <param name="source">The source table.</param>
        /// <returns>A builder object allowing to select the comment operation or set the rows for the operation.</returns>
        ICommentOperationOrRowSelector With(TomlTable source);
    }

    /// <summary>
    /// Builder interface to select the rows the combine operation will process.
    /// </summary>
    public interface IRowSelector
    {
        /// <summary>
        /// All rows existing in source will be processed. New rows will be added to the result table. Rows already existing in
        /// the target table will be replaced.
        /// </summary>
        /// <returns>The built combine operation.</returns>
        ITableCombiner ForAllSourceRows();

        /// <summary>
        /// All rows in the target table will be overwritten by the equivalent rows from the source table.
        /// </summary>
        /// <returns>The built combine operation.</returns>
        ITableCombiner ForAllTargetRows();

        /// <summary>
        /// Adds all existing rows from the target table and rows not existing yet in the target table to the resulting table.
        /// Effectively this is an add new rows operation.
        /// </summary>
        /// <returns>The built combine operation.</returns>
        ITableCombiner ForRowsOnlyInSource();
    }

    /// <summary>
    /// Builder interface to select how comments should be handled or what rows should get processed by the combine operation.
    /// </summary>
    public interface ICommentOperationOrRowSelector : IRowSelector
    {
        /// <summary>
        /// Replace or append comments in the target table with comments from the source table for rows that are processed.
        /// </summary>
        /// <param name="append">If <b>true</b> appends comments instead of replacing the target table comments.</param>
        /// <returns>A builder object that allows to select the rows to process.</returns>
        IRowSelector IncludingAllComments(bool append = false);

        /// <summary>
        /// Copies comments from the source table if the target table row does not have any comments yet.
        /// </summary>
        /// <returns>A builder object that allows to select the rows to process.</returns>
        IRowSelector IncludingNewComments();

        /// <summary>
        /// Keep comments from the target table and do not copy comments from the source table to the resulting table.
        /// </summary>
        /// <returns>A builder object that allows to select the rows to process.</returns>
        IRowSelector ExcludingComments();

        /// <summary>
        /// Specify custom func to control how comments should be processed.
        /// </summary>
        /// <param name="combiner">A func to control how comments should be handled.</param>
        /// <returns>A builder object that allows to select the rows to process.</returns>
        /// <exception cref="ArgumentNullException">If <i>combiner</i> is <b>null</b>.</exception>
        IRowSelector IncludingComments(CommentsOp combiner);
    }

    internal interface ITableOperation : ITableCombiner
    {
        TomlTable Execute();
    }

    /// <summary>
    /// Struct representing the input data for a custom comment combiner operation.
    /// </summary>
    /// <remarks>
    /// Used as the input argument for a custom comment handler func for
    /// <see cref="ICommentOperationOrRowSelector.IncludingComments(CommentsOp)"/>
    /// </remarks>
    public struct InputComments
    {
        internal InputComments(IEnumerable<TomlComment> tgt, IEnumerable<TomlComment> src)
        {
            this.TargetComments = tgt;
            this.SourceComments = src;
        }

        /// <summary>
        /// Gets the comments of the target table for the row being processed.
        /// Will be an empty collection for rows that do not exist in the target table.
        /// </summary>
        public IEnumerable<TomlComment> TargetComments { get; internal set; }

        /// <summary>
        /// Gets the comment of the source table for the row being processed.
        /// Will be and empty collection for rows that do not exist in the source table.
        /// </summary>
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
