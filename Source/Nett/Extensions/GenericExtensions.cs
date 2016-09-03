namespace Nett.Extensions
{
    using System;

    internal static class GenericExtensions
    {
        public static T CheckNotNull<T>(this T toCheck, string argName)
            where T : class
        {
            if (toCheck == null) { throw new ArgumentNullException(argName); }

            return toCheck;
        }
    }
}
