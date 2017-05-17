namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using Nett.Extensions;

    internal sealed class Transaction : IMergeConfigStore, IDisposable
    {
        private readonly Action<IMergeConfigStore> onCloseTransactionCallback;
        private readonly IMergeConfigStore persistable;
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

        public IEnumerable<IConfigSource> Sources => this.persistable.Sources;

        public string Alias => this.persistable.Alias;

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
            this.onCloseTransactionCallback(this.persistable);
        }

        public bool EnsureExists(TomlTable content) => false;

        public TomlTable Load() => this.transactionTable;

        public TomlTable Load(TomlTable table, IConfigSource source)
        {
            throw new NotImplementedException();
        }

        public TomlTable Load(IConfigSource source)
        {
            throw new NotImplementedException();
        }

        public TomlTable LoadSourcesTable() => this.transactionSourcesTable;

        public void Save(TomlTable content) => this.transactionTable = content;

        public void Save(TomlTable table, IConfigSource source)
        {
            throw new NotImplementedException();
        }

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void Init()
        {
            this.transactionTable = this.persistable.Load();
            this.transactionSourcesTable = this.persistable.Load();
        }
    }
}
