namespace Nett.Coma
{
    using System;
    using Nett.Extensions;

    internal sealed class Transaction : IMergeableConfig, IDisposable
    {
        private readonly Action<IMergeableConfig> onCloseTransactionCallback;
        private readonly IMergeableConfig persistable;
        private TomlTable transactionTable;
        private TomlTable transactionSourcesTable;

        private Transaction(IMergeableConfig persistable, Action<IMergeableConfig> onCloseTransactionCallback)
        {
            if (persistable is Transaction)
            {
                throw new InvalidOperationException(
                    "Cannot start new transaction as a transaction is already active. Only one active transaction is allowed.");
            }

            this.persistable = persistable.CheckNotNull(nameof(persistable));
            this.onCloseTransactionCallback = onCloseTransactionCallback.CheckNotNull(nameof(onCloseTransactionCallback));
        }

        public static Transaction Start(IMergeableConfig persistable, Action<IMergeableConfig> onCloseTransactionCallback)
        {
            var transaction = new Transaction(persistable, onCloseTransactionCallback);
            transaction.Init();
            return transaction;
        }

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

        public TomlTable LoadSourcesTable() => this.transactionSourcesTable;

        public void Save(TomlTable content) => this.transactionTable = content;

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void Init()
        {
            this.transactionTable = this.persistable.Load();
            this.transactionSourcesTable = this.persistable.Load();
        }

        public TomlTable Load(TomlTable table, IConfigSource source)
        {
            throw new NotImplementedException();
        }

        public void Save(TomlTable table, IConfigSource source)
        {
            throw new NotImplementedException();
        }

        public TomlTable Load(IConfigSource source)
        {
            throw new NotImplementedException();
        }

        public bool CanHandleSource(IConfigSource source) => this.persistable.CanHandleSource(source);
    }
}
