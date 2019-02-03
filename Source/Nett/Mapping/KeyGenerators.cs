using System.Reflection;
using System.Text;

namespace Nett
{
    public sealed class KeyGenerators
    {
        internal static readonly KeyGenerators Instance = new KeyGenerators();

        private static readonly IKeyGenerator PropertyNameGenerator = new PropertyNameKeyGenerator();
        private static readonly IKeyGenerator UpperCaseGenerator = new UpperCaseKeyGenerator();
        private static readonly IKeyGenerator LowerCaseGenerator = new LowerCaseKeyGenerator();
        private static readonly IKeyGenerator CamelCaseGenerator = new CamelCaseKeyGenerator();
        private static readonly IKeyGenerator PascalCaseGenerator = new PascalCaseKeyGenerator();

        private KeyGenerators()
        {
        }

        public IKeyGenerator PropertyName
            => PropertyNameGenerator;

        public IKeyGenerator UpperCase
            => UpperCaseGenerator;

        public IKeyGenerator LowerCase
            => LowerCaseGenerator;

        public IKeyGenerator CamelCase
            => CamelCaseGenerator;

        public IKeyGenerator PascalCase
            => PascalCaseGenerator;

        private sealed class PropertyNameKeyGenerator : IKeyGenerator
        {
            public string GetKey(MemberInfo property)
                => property.Name;
        }

        private sealed class UpperCaseKeyGenerator : IKeyGenerator
        {
            public string GetKey(MemberInfo property)
                => property.Name.ToUpperInvariant();
        }

        private sealed class LowerCaseKeyGenerator : IKeyGenerator
        {
            public string GetKey(MemberInfo property)
                => property.Name.ToLowerInvariant();
        }

        private sealed class CamelCaseKeyGenerator : IKeyGenerator
        {
            public string GetKey(MemberInfo property)
            {
                string name = property.Name;
                if (char.IsUpper(name[0])) { return name; }

                var sb = new StringBuilder(name);
                sb[0] = char.ToUpperInvariant(sb[0]);
                return sb.ToString();
            }
        }

        private sealed class PascalCaseKeyGenerator : IKeyGenerator
        {
            public string GetKey(MemberInfo property)
            {
                string name = property.Name;
                if (char.IsLower(name[0])) { return name; }

                var sb = new StringBuilder(name);
                sb[0] = char.ToLowerInvariant(sb[0]);
                return sb.ToString();
            }
        }
    }
}
