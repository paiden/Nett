using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Nett.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public sealed class FFact : FactAttribute
    {
        public FFact(string functionality, string message)
        {
            this.DisplayName = $"{functionality}: {message}";
        }
    }
}
