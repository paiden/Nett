using System.Collections.Generic;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed partial class InlineTableTests
    {
        [Fact]
        public void Write_WhenDictionaryValueTypeIsConfiguredAsInlineTable_ThatTypeIsWrittenAsAnInlineTable()
        {
            var config = TomlSettings.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(ItemDict.TwoItems, config);

            s.ShouldBeNormalizedEqualTo(ItemDict.TwoItemsInlineSerialzed);
        }

        [Fact]
        public void Write_WhenDictIsMarkedAsInline_WritesDictAsInlineAndItemsAutomaticallyAsNestedInlineTables()
        {
            var config = TomlSettings.Create(cfg =>
                cfg.ConfigureType<Dictionary<string, Item>>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(ItemDict.TwoItems, config);

            s.ShouldBeNormalizedEqualTo(ItemDict.TwoItemsDictInlineSerialized);
        }

        [Fact]
        public void Write_WithEmptyInlineTableArray_WritesNothingToTheFile()
        {
            var config = TomlSettings.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(InlineArray.Empty, config);

            s.ShouldBeNormalizedEqualTo(InlineArray.ExpectedEmpty);
        }

        [Fact]
        public void Write_WhenTblArrayValueTypeIsConfiguredAsInlineTable_ThatTypeIsWrittenAsAnInlineTable()
        {
            var config = TomlSettings.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(InlineArray.TwoItems, config);

            s.ShouldBeNormalizedEqualTo(InlineArray.ExpectedTwoItems);
        }



        [Fact]
        public void Write_GivenItemsThatAreInlinedViaClassAttribute_WritesThatItemsAsInlineStructures()
        {
            var s = Toml.WriteString(InlineArrayAttributeOnItem.TwoItems);

            s.ShouldBeNormalizedEqualTo(InlineArrayAttributeOnItem.TowItemsSerialized);
        }

        [Fact]
        public void GivenSringBoolDictConfig_WhenWrittenAsToml_WritesTheDictAsInlineTable()
        {
            var s = Toml.WriteString(new ConfigWithStringBoolDict());

            s.ShouldBeNormalizedEqualTo(ConfigWithStringBoolDict.Serialized);
        }
    }
}
