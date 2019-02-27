namespace Nett.Exp
{
    public static class TomlValueExtensions
    {
        public static string GetUnit(this TomlValue value, string def = "")
            => value.Unit ?? def;
    }
}
