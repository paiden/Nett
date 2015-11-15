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
        internal static readonly TomlConfig DefaultInstance = Create();

        private readonly Dictionary<Type, ITomlConverter> fromTomlConverters = new Dictionary<Type, ITomlConverter>();
        private readonly Dictionary<Type, ITomlConverter> toTomlConverters = new Dictionary<Type, ITomlConverter>();
        private readonly Dictionary<Type, Func<object>> activators = new Dictionary<Type, Func<object>>();
        private readonly HashSet<Type> inlineTableTypes = new HashSet<Type>();

        private TomlCommentLocation DefaultCommentLocation = TomlCommentLocation.Prepend;
        private TomlConfig()
        {

        }

        public static TomlConfig Create() => new TomlConfig();

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

        internal object GetActivatedInstance(Type t)
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
                    throw new InvalidOperationException(string.Format("Failed to create type '{1}'. Only types with a " +
                        "parameterless constructor or an specialized creator can be created. Make sure the type has " +
                        "a parameterless constructor or a configuration with an corresponding creator is provided.", exc.Message, t.FullName));
                }
            }
        }

        internal TomlTable.TableTypes GetTableType(PropertyInfo pi) =>
            this.inlineTableTypes.Contains(pi.PropertyType) ||
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
