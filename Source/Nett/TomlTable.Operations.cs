using Nett.Extensions;

namespace Nett
{
    public interface ITableCombiner
    {
    }

    public interface ITargetSelector
    {
        ISourceSelector Overwrite(TomlTable target);
    }

    public interface ISourceSelector
    {
        IRowSelector With(TomlTable source);
    }

    public interface IRowSelector
    {
        ITableCombiner ForAllSourceRows();

        ITableCombiner ForAllTargetRows();

        ITableCombiner ForRowsOnlyInSource();
    }

    internal interface ITableOperation : ITableCombiner
    {
        TomlTable Execute();
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

        internal sealed class OverwriteTableOperationBuilder : ISourceSelector, IRowSelector
        {
            private readonly TomlTable target;
            private TomlTable source;

            public OverwriteTableOperationBuilder(TomlTable target)
            {
                this.target = target;
            }

            ITableCombiner IRowSelector.ForAllSourceRows()
                => new OverwriteAllSourceRowsOperation(this.target, this.source);

            ITableCombiner IRowSelector.ForRowsOnlyInSource()
                => new OverwriteSourceOnlyRowsOperation(this.target, this.source);

            ITableCombiner IRowSelector.ForAllTargetRows()
                => new OverwriteAllTargetRowsOperation(this.target, this.source);

            IRowSelector ISourceSelector.With(TomlTable source)
            {
                this.source = source;
                return this;
            }
        }

        internal abstract class TableCombineOperation : ITableOperation
        {
            protected readonly TomlTable target;
            protected readonly TomlTable source;

            public TableCombineOperation(TomlTable target, TomlTable source)
            {
                this.target = target.CheckNotNull(nameof(target));
                this.source = source.CheckNotNull(nameof(source));
            }

            public virtual TomlTable Execute() => this.target.CloneTableFor(this.target.Root);
        }

        internal sealed class OverwriteAllSourceRowsOperation : TableCombineOperation
        {
            public OverwriteAllSourceRowsOperation(TomlTable target, TomlTable source)
                : base(target, source)
            {
            }

            public sealed override TomlTable Execute()
            {
                var combineResult = base.Execute();

                foreach (var r in this.source.rows)
                {
                    combineResult.rows[r.Key] = r.Value.CloneFor(combineResult.Root);
                }

                return combineResult;
            }
        }

        internal sealed class OverwriteAllTargetRowsOperation : TableCombineOperation
        {
            public OverwriteAllTargetRowsOperation(TomlTable target, TomlTable source)
                : base(target, source)
            {
            }

            public sealed override TomlTable Execute()
            {
                var combineResult = base.Execute();

                foreach (var tr in this.target.rows)
                {
                    if (this.source.rows.TryGetValue(tr.Key, out var sr))
                    {
                        combineResult.rows[tr.Key] = sr.CloneFor(combineResult.Root);
                    }
                }

                return combineResult;
            }
        }

        internal sealed class OverwriteSourceOnlyRowsOperation : TableCombineOperation
        {
            public OverwriteSourceOnlyRowsOperation(TomlTable target, TomlTable source)
                : base(target, source)
            {
            }

            public sealed override TomlTable Execute()
            {
                var combineResult = base.Execute();

                foreach (var sr in this.source.rows)
                {
                    if (!combineResult.rows.ContainsKey(sr.Key))
                    {
                        combineResult.rows[sr.Key] = sr.Value.CloneFor(combineResult.Root);
                    }
                }

                return combineResult;
            }
        }
    }
}
