using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public sealed partial class InlineTableTests
    {
        [Fact]
        public void Write_WhenDictionaryValueTypeIsConfiguredAsInlineTable_ThatTypeIsWrittenAsAnInlineTable()
        {
            var config = TomlConfig.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(new InlineDict(), config);

            s.Should().Be(InlineDict.Expected);
        }

        [Fact]
        public void Write_WithEmptyInlineTableArray_WritesNothingToTheFile()
        {
            var config = TomlConfig.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(InlineArray.Empty, config);

            s.Should().Be(InlineArray.ExpectedEmpty);
        }

        [Fact]
        public void Write_WhenTblArrayValueTypeIsConfiguredAsInlineTable_ThatTypeIsWrittenAsAnInlineTable()
        {
            var config = TomlConfig.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(InlineArray.TwoItems, config);

            s.Should().Be(InlineArray.ExpectedTwoItems);
        }

        [Fact]
        public void Write_GivenRootItemThatInlinesDictViaPropertyAttribute_WritesTheDictAndItemsAsInlineItems()
        {
            var s = Toml.WriteString(InlineDictViaAttribute.TwoItems);

            s.Should().Be(InlineDictViaAttribute.TwoItemsSerialized);
        }

        [Fact]
        public void Write_GivenItemsThatAreInlinedViaClassAttribute_WritesThatItemsAsInlineStructures()
        {
            var s = Toml.WriteString(InlineArrayAttributeOnItem.TwoItems);

            s.Should().Be(InlineArrayAttributeOnItem.TowItemsSerialized);
        }
    }
}
