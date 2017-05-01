using Nett.UnitTests.Util;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public sealed partial class InlineTableTests
    {
        [Fact]
        public void Read_WhenStringContainsDictWithInlineTables_ProducesCorrectInMemoryStructure()
        {
            var read = Toml.ReadString<InlineDict>(InlineDict.Expected);

            read.SouldBeEqualByJsonCompare(new InlineDict());
        }

        [Fact]
        public void Read_GivenSerialzedContent_WhenReadIntoAttributedDataStructure_ProducesCorrectInMemoryRepresenation()
        {
            var read = Toml.ReadString<InlineDict>(InlineDictViaAttribute.TwoItemsSerialized);

            read.SouldBeEqualByJsonCompare(InlineDictViaAttribute.TwoItems);
        }
    }
}
