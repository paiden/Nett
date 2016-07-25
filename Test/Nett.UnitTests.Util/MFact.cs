using Xunit;

namespace Nett.UnitTests.Util
{
    public sealed class MFact : FactAttribute
    {
        public MFact(string testedClass, string testedMember, string displayMessage)
        {
            this.DisplayName = $"{testedClass}.{testedMember}: {displayMessage}";
        }
    }
}
