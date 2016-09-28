namespace Nett.Coma
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using Nett.Extensions;

    public sealed class Config<T>
        where T : class
    {
        private readonly Config config;

        internal Config(IMergeableConfig persistable)
        {
            this.config = new Config(persistable);
        }

        public TRet Get<TRet>(Expression<Func<T, TRet>> selector)
        {
            if (selector == null) { throw new ArgumentNullException(nameof(selector)); }

            return this.config.Get(tbl =>
            {
                var keyChain = selector.ResolveKeyChain();
                var target = keyChain.ResolveChildTableOf(tbl);
                return target[keyChain.TargetTableKey].Get<TRet>();
            });
        }

        public bool Clear<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            var path = selector.CheckNotNull(nameof(selector)).BuildTPath();
            return this.config.Clear(path);
        }

        public IConfigSource GetSource<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.config.TryGetSource(selector.BuildTPath());
        }

        public void Set<TProperty>(Expression<Func<T, TProperty>> selector, TProperty value)
        {
            var keyChain = selector.CheckNotNull(nameof(selector)).ResolveKeyChain();
            this.config.Set(table => this.ApplySetter(table, keyChain, value));
        }

        private void ApplySetter(TomlTable rootTable, KeyChain keyChain, object value)
        {
            var finalTable = keyChain.ResolveChildTableOf(rootTable);
            finalTable[keyChain.TargetTableKey] = TomlValue.CreateFrom(rootTable.Root, value, pi: null);
        }

        private void ApplySetter(TomlTable rootTable, KeyChain keyChain, object value, IConfigSource source)
        {
            var finalTable = keyChain.ResolveChildTableOf(rootTable);
            finalTable[keyChain.TargetTableKey] = TomlValue.CreateFrom(rootTable.Root, value, pi: null);
        }

        public void Set<TProperty>(Expression<Func<T, TProperty>> selector, TProperty value, IConfigSource target)
        {
            var keyChain = selector.CheckNotNull(nameof(selector)).ResolveKeyChain();
            this.config.Set(table => this.ApplySetter(table, keyChain, value), target);
        }

        public IDisposable StartTransaction() => this.config.StartTransaction();

        public T Unmanaged() => this.config.Unmanaged().Get<T>();


    }
}
