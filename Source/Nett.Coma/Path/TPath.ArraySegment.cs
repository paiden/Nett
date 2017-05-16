namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private class ArraySegment : KeySegment
        {
            public ArraySegment(string key)
                : base(TomlObjectType.Array, key)
            {
            }

            public override string ToString() => $"/[{this.key}]";
        }
    }
}
