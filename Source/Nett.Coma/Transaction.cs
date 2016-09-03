namespace Nett.Coma
{
    using System;
    using Extensions;

    internal sealed class Transaction : IPersistableConfig, IDisposable
    {
        private readonly Action<IPersistableConfig> onCloseTransactionCallback;
        private readonly IPersistableConfig persistable;
        private TomlTable transactionTable;

        private Transaction(IPersistableConfig persistable, Action<IPersistableConfig> onCloseTransactionCallback)
        {
            if (persistable is Transaction)
            {
                throw new InvalidOperationException(
                    "Cannot start new transaction as a transaction is already active. Only one active transaction is allowed.");
            }

            this.persistable = persistable.CheckNotNull(nameof(persistable));
            this.onCloseTransactionCallback = onCloseTransactionCallback.CheckNotNull(nameof(onCloseTransactionCallback));
        }

        public static Transaction Start(IPersistableConfig persistable, Action<IPersistableConfig> onCloseTransactionCallback)
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

        public void Save(TomlTable content) => this.transactionTable = content;

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        private void Init()
        {
            this.transactionTable = this.persistable.Load();
        }
    }
}
