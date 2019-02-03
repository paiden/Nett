using System;

namespace Nett
{
    public sealed class TomlMember : Attribute
    {
        public TomlMember()
        {
        }

        public string Key { get; set; } = null;
    }
}
