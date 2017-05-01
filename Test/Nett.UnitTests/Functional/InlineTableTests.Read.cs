using Nett.UnitTests.Util;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public sealed partial class InlineTableTests
    {
        [Fact]
        public void Read_WhenStringContainsDictWithInlineTables_ProducesCorrectInMemoryStructure()
        {
            var read = Toml.ReadString<ItemDict>(ItemDict.TwoItemsInlineSerialzed);

            read.SouldBeEqualByJsonCompare(ItemDict.TwoItems);
        }
    }
}
