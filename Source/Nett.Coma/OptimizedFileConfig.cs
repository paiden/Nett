namespace Nett.Coma
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Nett.Extensions;

    internal sealed class OptimizedFileConfig : IPersistableConfig
    {
        private readonly ExecuteLastOnly writeOptimizer;
        private readonly FileConfig persistable;

        private TomlTable loaded = null;

        public OptimizedFileConfig(FileConfig persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));
            this.writeOptimizer = new ExecuteLastOnly(TimeSpan.FromMilliseconds(500));
        }

        public bool EnsureExists(TomlTable content) => this.persistable.EnsureExists(content);

        public TomlTable Load()
        {
            if (this.loaded == null || this.persistable.WasChangedExternally())
            {
                this.loaded = this.persistable.Load();
            }

            return this.loaded;
        }

        public void Save(TomlTable content)
        {
            this.writeOptimizer.Execute(() => this.persistable.Save(content));
        }

        public bool WasChangedExternally() => this.persistable.WasChangedExternally();

        public void Dispose() => this.writeOptimizer.Dispose();

        private class ExecuteLastOnly : IDisposable
        {
            private readonly TimeSpan waitInterval;

            private CancellationTokenSource cancel = new CancellationTokenSource();
            private Task task;

            public ExecuteLastOnly(TimeSpan waitInterval)
            {
                this.waitInterval = waitInterval;
            }

            public void Execute(Action toExecute)
            {
                this.cancel.Cancel();
                this.cancel.Dispose();
                this.cancel = new CancellationTokenSource();
                this.task = Task.Delay(this.waitInterval, this.cancel.Token)
                    .ContinueWith(_ => toExecute);
            }

            public void Dispose() => this.cancel.Dispose();
        }
    }
}
