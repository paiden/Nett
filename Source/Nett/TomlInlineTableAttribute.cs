namespace Nett
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class TomlInlineTableAttribute : Attribute
    {
    }
}
