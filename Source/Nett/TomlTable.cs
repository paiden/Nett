using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public sealed class TomlTable : TomlObject
    {
        public string Name { get; private set; } = "";
        public Dictionary<string, TomlObject> Rows { get; } = new Dictionary<string, TomlObject>();

        internal bool IsDefined { get; set; }

        public TomlTable(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            this.Name = name;
        }

        public TomlObject this[string key]
        {
            get { return this.Rows[key]; }
        }

        public T Get<T>(string key)
        {
            return this[key].Get<T>();
        }

        public void Add(string key, TomlObject value)
        {
            this.Rows.Add(key, value);
        }

        public override T Get<T>()
        {
            throw new NotImplementedException();
        }

        public override object Get(Type t)
        {
            throw new NotImplementedException();
        }

        public T MapTo<T>()
        {
            var t = typeof(T);
            var result = Activator.CreateInstance<T>();

            foreach(var p in this.Rows)
            {
                var targetProperty = t.GetProperty(p.Key);
                if(targetProperty != null)
                {
                    targetProperty.SetValue(result, p.Value.Get(targetProperty.PropertyType), null);
                }
            }

            return result;
        }
    }
}
