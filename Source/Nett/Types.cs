namespace Nett
{
    using System;
    using System.Collections;

    internal static class Types
    {
        public static readonly Type EnumType = typeof(Enum);
        public static readonly Type ListType = typeof(IList);
        public static readonly Type ObjectType = typeof(object);
        public static readonly Type StringType = typeof(string);
        public static readonly Type TomlArrayType = typeof(TomlArray);
        public static readonly Type TomlObjectType = typeof(TomlObject);
        public static readonly Type TomlStringType = typeof(TomlString);
        public static readonly Type TomlTableType = typeof(TomlTable);
        public static readonly Type TomlValueType = typeof(TomlValue);
    }
}
