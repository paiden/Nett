using Xunit;

namespace Nett.UnitTests.Util
{
    public sealed class FFact : FactAttribute
    {
        public FFact(string functionality, string message)
        {
            this.DisplayName = $"{functionality}: {message}";
        }
    }
}
