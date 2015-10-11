using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nett
{
    enum TomlCommentLocation
    {
        Prepend,
        Append,
    }

    public sealed partial class TomlConfig
    {
        internal static readonly TomlConfig DefaultInstance = Default();
        private readonly Dictionary<Type, ITomlConverter> fromTomlConverters = new Dictionary<Type, ITomlConverter>();
        private readonly Dictionary<Type, ITomlConverter> toTomlConverters = new Dictionary<Type, ITomlConverter>();
        private readonly Dictionary<Type, Func<object>> activators = new Dictionary<Type, Func<object>>();
        private TomlCommentLocation DefaultCommentLocation = TomlCommentLocation.Prepend;
        private TomlConfig()
        {

        }

        public static TomlConfig Default() => new TomlConfig();

        public TomlConfig AddConverter(ITomlConverter converter)
        {
            if (typeof(TomlObject).IsAssignableFrom(converter.ToType))
            {
                this.toTomlConverters.Add(converter.FromType, converter);
            }
            else
            {
                this.fromTomlConverters.Add(converter.ToType, converter);
            }

            return this;
        }

        public TomlConfig AddActivator<T>(Func<object> activator)
        {
            this.activators.Add(typeof(T), activator);
            return this;
        }

        internal ITomlConverter GetFromTomlConverter(Type toType)
        {
            ITomlConverter conv;
            this.fromTomlConverters.TryGetValue(toType, out conv);
            return conv;
        }

        internal ITomlConverter GetToTomlConverter(Type fromType)
        {
            ITomlConverter conv;
            this.toTomlConverters.TryGetValue(fromType, out conv);
            return conv;
        }

        public object GetActivatedInstance(Type t)
        {
            Func<object> a;
            if (this.activators.TryGetValue(t, out a))
            {
                return a();
            }
            else
            {
                try
                {
                    return Activator.CreateInstance(t);
                }
                catch (MissingMethodException exc)
                {
                    throw new Exception(string.Format("{0} Failed to create type '{1}'.", exc.Message, t.FullName));
                }
            }
        }

        internal TomlTable.TableTypes GetTableType(PropertyInfo pi) =>
            pi.GetCustomAttributes(false).Any((a) => a.GetType() == typeof(TomlInlineTableAttribute)) ? TomlTable.TableTypes.Inline : TomlTable.TableTypes.Default;

        internal TomlCommentLocation GetCommentLocation(TomlComment c)
        {
            switch (c.Location)
            {
                case CommentLocation.Append: return TomlCommentLocation.Append;
                case CommentLocation.Prepend: return TomlCommentLocation.Prepend;
                default: return DefaultCommentLocation;
            }
        }
    }
}
