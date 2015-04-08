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

        public ITomlConverter GetConverter(Type targetType)
        {
            ITomlConverter conv;
            this.converters.TryGetValue(targetType, out conv);
            return conv;
        }
    }
}
