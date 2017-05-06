using FluentAssertions;
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

        [Fact]
        public void GivenTomlWithInlineTable_WhenRead_ProducesInMemoryInlineTable()
        {
            var read = Toml.ReadString("X = { 'A' = true, 'B' = false }");

            read.Get<TomlTable>("X").TableType.Should().Be(TomlTable.TableTypes.Inline);
        }
    }
}
