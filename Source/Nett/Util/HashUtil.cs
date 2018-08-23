namespace Nett.Util
{
    internal static class HashUtil
    {
        public static int GetHashCode<T1, T2>(T1 arg1, T2 arg2)
        {
            unchecked
            {
                var h1 = arg1?.GetHashCode() ?? 0;
                var h2 = arg2?.GetHashCode() ?? 0;
                return (31 * h1) + h2;
            }
        }
    }
}
