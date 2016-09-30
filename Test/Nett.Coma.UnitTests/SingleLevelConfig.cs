using System.Diagnostics.CodeAnalysis;

namespace Nett.Coma.Tests
{
    [ExcludeFromCodeCoverage]
    public sealed class SingleLevelConfig
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
    }
}
