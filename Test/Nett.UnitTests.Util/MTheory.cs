using Xunit;

namespace Nett.UnitTests.Util
{
    public sealed class MTheory : TheoryAttribute
    {
        public MTheory(string testedClass, string testedMember, string displayMessage)
        {
            this.DisplayName = $"{testedClass}.{testedMember}: {displayMessage}";
        }
    }
}
