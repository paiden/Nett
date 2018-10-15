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
        public static readonly Type TomlBoolType = typeof(TomlBool);
        public static readonly Type TomlStringType = typeof(TomlString);
        public static readonly Type TomlIntType = typeof(TomlInt);
        public static readonly Type TomlFloatType = typeof(TomlFloat);
        public static readonly Type TomlDateTimeType = typeof(TomlOffsetDateTime);
        public static readonly Type TomlTimeSpanType = typeof(TomlDuration);
        public static readonly Type TomlTableType = typeof(TomlTable);
        public static readonly Type TomlValueType = typeof(TomlValue);
        public static readonly Type TomlTableArrayType = typeof(TomlTableArray);
        public static readonly Type TomlLocalDateTime = typeof(TomlLocalDateTime);
        public static readonly Type TomlLocalDate = typeof(TomlLocalDate);
        public static readonly Type TomlLocalTime = typeof(TomlLocalTime);
    }
}
