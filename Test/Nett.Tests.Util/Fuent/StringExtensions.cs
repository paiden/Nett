using Nett.Tests.Util;

namespace Nett.Tests
{
    public static class StringExtensions
    {
        /// <summary>
        /// Should help to do assertions so that the type of newline the code file has
        /// does not matter anymore. Some users reported failing tests in cross platform
        /// scenarios. And although editor config defines what new line type to use,
        /// sometimes for whatever reason files still have different new lines.
        /// </summary>
        public static NormalizedStringAssertions ShouldNormalized(this string instance)
        {
            instance = instance?.NormalizeLineEndings()?.Trim();

            return new NormalizedStringAssertions(instance);
        }
    }
}
