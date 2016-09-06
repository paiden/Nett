namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public sealed class Config<T>
        where T : class
    {
        private readonly Config config;

        internal Config(IMergeableConfig persistable)
        {
            this.config = new Config(persistable);
        }

        public TRet Get<TRet>(Func<T, TRet> getter)
        {
            if (getter == null) { throw new ArgumentNullException(nameof(getter)); }

            return this.config.Get(tbl => getter(tbl.Get<T>()));
        }

        public IConfigSource GetSource(Expression<Func<T, object>> getter)
        {
            var keyChain = ResolveTableKeys(getter);
            return this.config.GetSource(keyChain);
        }

        public void Set(Action<T> setter)
        {
            if (setter == null) { throw new ArgumentNullException(nameof(setter)); }

            this.config.SetInternal((ref TomlTable tbl) =>
            {
                var typedTable = tbl.Get<T>();
                setter(typedTable);
                tbl = Toml.Create(typedTable);
            });
        }

        public IDisposable StartTransaction() => this.config.StartTransaction();

        public T Unmanaged() => this.config.Unmanaged().Get<T>();

        private static List<string> ResolveTableKeys(Expression<Func<T, object>> expr)
        {
            List<string> keys = new List<string>();
            MemberExpression me;

            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;
                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            while (me != null)
            {
                keys.Insert(0, me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            return keys;
        }
    }
}
