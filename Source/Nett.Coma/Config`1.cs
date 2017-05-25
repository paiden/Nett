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

        internal Config(IMergeConfigStore persistable)
        {
            this.config = new Config(persistable);
        }

        public TRet Get<TRet>(Expression<Func<T, TRet>> selector)
        {
            selector.CheckNotNull(nameof(selector));

            var obj = this.config.GetFromPath(selector.BuildTPath());
            return obj.Get<TRet>();
        }

        public bool Clear<TProperty>(Expression<Func<T, TProperty>> selector)
        {
            var path = selector.CheckNotNull(nameof(selector)).BuildTPath();
            return this.config.Clear(path);
        }

        public bool Clear<TProperty>(Expression<Func<T, TProperty>> selector, IConfigSource source)
        {
            var path = selector.CheckNotNull(nameof(selector)).BuildTPath();
            return this.config.Clear(path, source);
        }

        public IConfigSource GetSource<TResult>(Expression<Func<T, TResult>> selector)
        {
            return this.config.TryGetSource(selector.BuildTPath());
        }

        public void Set<TProperty>(Expression<Func<T, TProperty>> selector, TProperty value)
        {
            selector.CheckNotNull(nameof(selector));

            var path = selector.BuildTPath();
            this.config.Set(path, value);
        }

        public void Set<TProperty>(Expression<Func<T, TProperty>> selector, TProperty value, IConfigSource target)
        {
            selector.CheckNotNull(nameof(selector));
            target.CheckNotNull(nameof(target));

            var path = selector.BuildTPath();
            this.config.Set(path, value, target);
        }

        public IDisposable StartTransaction() => this.config.StartTransaction();

        public T Unmanaged() => this.config.Unmanaged().Get<T>();
    }
}
