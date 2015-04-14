using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public class TomlConfig
    {
        internal static readonly TomlConfig DefaultInstance = Default();
        private readonly Dictionary<Type, ITomlConverter> converters = new Dictionary<Type, ITomlConverter>();
        private readonly Dictionary<Type, Func<object>> activators = new Dictionary<Type, Func<object>>();

        private TomlConfig()
        {

        }

        public static TomlConfig Default()
        {
            return new TomlConfig();
        }

        public TomlConfig AddConverter(ITomlConverter converter)
        {
            this.converters.Add(converter.TargetType, converter);
            return this;
        }

        public TomlConfig AddActivator<T>(Func<object> activator)
        {
            this.activators.Add(typeof(T), activator);
            return this;
        }

        public ITomlConverter GetConverter(Type targetType)
        {
            ITomlConverter conv;
            this.converters.TryGetValue(targetType, out conv);
            return conv;
        }

        public object GetActivatedInstance(Type t)
        {
            Func<object> a;
            if(this.activators.TryGetValue(t, out a))
            {
                return a();
            }
            else
            {
                try
                {
                    return Activator.CreateInstance(t);
                }
                catch(MissingMethodException exc)
                {
                    throw new Exception(string.Format("{0} Failed to create type '{1}'.", exc.Message, t.FullName));
                }
            }
        }
    }
}
