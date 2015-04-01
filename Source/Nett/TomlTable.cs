using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

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
            return (T)this.Get(typeof(T));
        }

        public override object Get(Type t)
        {
            var result = Activator.CreateInstance(t);

            foreach (var p in this.Rows)
            {
                var targetProperty = t.GetProperty(p.Key);
                if (targetProperty != null)
                {
                    targetProperty.SetValue(result, p.Value.Get(targetProperty.PropertyType), null);
                }
            }

            return result;
        }

        public static TomlTable From<T>(T obj)
        {
            TomlTable tt = new TomlTable("");

            var t = obj.GetType();
            var props = t.GetProperties();

            foreach(var p in props)
            {
                object val = p.GetValue(obj, null);
                tt.Add(p.Name, TomlValue.From(val, p.PropertyType));
            }

            return tt;
        }

        public override void WriteTo(StreamWriter sw)
        {
            foreach(var r in this.Rows)
            {
                sw.Write("{0} = ", r.Key);
                r.Value.WriteTo(sw);
                sw.WriteLine("");
            }
        }
    }
}
