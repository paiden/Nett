namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using Nett.Extensions;

    internal sealed class Transaction : IMergeConfigStore, IDisposable
    {
        private readonly Action<IMergeConfigStore> onCloseTransactionCallback;
        private readonly IMergeConfigStore persistable;
        private readonly Dictionary<IConfigSource, TomlTable> perSourceTransactionTables
            = new Dictionary<IConfigSource, TomlTable>(new SourceEqualityComparer());

        private TomlTable transactionSourcesTable;
        private TomlTable transactionTable;

        private Transaction(IMergeConfigStore persistable, Action<IMergeConfigStore> onCloseTransactionCallback)
        {
            if (persistable is Transaction)
            {
                throw new InvalidOperationException(
                    "Cannot start new transaction as a transaction is already active. Only one active transaction is allowed.");
            }

            this.persistable = persistable.CheckNotNull(nameof(persistable));
            this.onCloseTransactionCallback = onCloseTransactionCallback.CheckNotNull(nameof(onCloseTransactionCallback));
        }

        public IConfigSource Source => this.persistable.Source;

        public IConfigSource RootSource => this.persistable.RootSource;

        public IEnumerable<IConfigSource> Sources => this.persistable.Sources;

        public static Transaction Start(IMergeConfigStore persistable, Action<IMergeConfigStore> onCloseTransactionCallback)
        {
            var transaction = new Transaction(persistable, onCloseTransactionCallback);
            transaction.Init();
            return transaction;
        }

        public bool CanHandleSource(IConfigSource source) => this.persistable.CanHandleSource(source);

        public void Dispose()
        {
            if (this.persistable.WasChangedExternally())
            {
                throw new InvalidOperationException("Cannot save config because it was modified externally.");
            }

            this.persistable.Save(this.transactionTable);

            foreach (var kvp in this.perSourceTransactionTables)
            {
                this.persistable.Save(kvp.Value, kvp.Key);
            }

            this.onCloseTransactionCallback(this.persistable);
        }

        public bool EnsureExists(TomlTable content) => false;

        public TomlTable Load() => this.transactionTable;

        public TomlTable Load(IConfigSource source) => this.perSourceTransactionTables[source];

        public TomlTable LoadSourcesTable() => this.transactionSourcesTable;

        public void RemoveEmptyTables()
            => this.persistable.RemoveEmptyTables();

        public void Save(TomlTable content) => this.transactionTable = content;

        public void Save(TomlTable table, IConfigSource source)
            => this.perSourceTransactionTables[source] = table;

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void Init()
        {
            this.transactionTable = this.persistable.Load();
            this.transactionSourcesTable = this.persistable.Load();

            foreach (var s in this.persistable.Sources)
            {
                this.perSourceTransactionTables[s] = this.persistable.Load(s);
            }
        }

        private sealed class SourceEqualityComparer : IEqualityComparer<IConfigSource>
        {
            public bool Equals(IConfigSource x, IConfigSource y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(IConfigSource obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}
