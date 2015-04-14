using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;

namespace Nett
{
    public sealed class TomlTable : TomlObject
    {
        private static readonly Type EnumerableType = typeof(IEnumerable);
        private static readonly Type StringType = typeof(string);
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
            return (T)this.Get<T>(TomlConfig.DefaultInstance);
        }

        public override T Get<T>(TomlConfig config)
        {
            return (T)this.Get(typeof(T), config);
        }

        public override object Get(Type t)
        {
            return this.Get(t, TomlConfig.DefaultInstance);
        }

        public override object Get(Type t, TomlConfig config)
        {
            var result = config.GetActivatedInstance(t);

            foreach (var p in this.Rows)
            {
                var targetProperty = result.GetType().GetProperty(p.Key);
                if (targetProperty != null)
                {
                    var converter = config.GetConverter(targetProperty.PropertyType);
                    if (converter != null)
                    {
                        var src = p.Value.Get(converter.SourceType);
                        var val = converter.FromToml(src);
                        targetProperty.SetValue(result, val, null);
                    }
                    else
                    {
                        targetProperty.SetValue(result, p.Value.Get(targetProperty.PropertyType, config), null);
                    }
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
                TomlObject to = null;
                if (p.PropertyType != StringType && EnumerableType.IsAssignableFrom(p.PropertyType))
                {
                    to = TomlArray.From((IEnumerable)val);
                }
                else
                {
                    to = TomlValue.From(val, p.PropertyType);
                }

                AddComments(to, p);
                tt.Add(p.Name, to);
            }

            return tt;
        }

        public override void WriteTo(StreamWriter sw)
        {
            this.WriteTo(sw, TomlConfig.DefaultInstance);
        }

        public override void WriteTo(StreamWriter sw, TomlConfig config)
        {
            foreach (var r in this.Rows)
            {
                WriteRowTo(sw, r, config);
            }
        }

        private static void WriteRowTo(StreamWriter sw, KeyValuePair<string, TomlObject> r, TomlConfig config)
        {
            var prependComments = r.Value.Comments.Where((c) => config.GetCommentLocation(c) == TomlCommentLocation.Prepend);
            var appendComments = r.Value.Comments.Where((c) => config.GetCommentLocation(c) == TomlCommentLocation.Append);

            foreach(var ppc in prependComments)
            {
                sw.WriteLine("# {0}", ppc.CommentText);
            }

                sw.Write("{0} = ", r.Key);
                r.Value.WriteTo(sw);

            foreach(var apc in appendComments)
            {
                sw.Write(" # {0}", apc.CommentText);
            }

                sw.WriteLine("");
            }

        private static void AddComments(TomlObject obj, PropertyInfo pi)
        {
            var comments = pi.GetCustomAttributes(typeof(TomlCommentAttribute), false).Cast<TomlCommentAttribute>();
            foreach(var c in comments)
            {
                obj.Comments.Add(new TomlComment(c.Comment, c.Location));
            }
        }
    }
}
