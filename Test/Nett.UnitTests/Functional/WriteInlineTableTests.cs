using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public sealed class WriteInlineTableTest
    {
        [Fact]
        public void WriteTable_WhenDictionaryValueTypeIsConfiguredAsInlineTable_ThatTypeIsWrittenAsAnInlineTable()
        {
            var config = TomlConfig.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(new RootWithDict(), config);

            s.Should().Be(RootWithDict.Expected);
        }

        [Fact]
        public void WriteTable_WithEmptyInlineTableArray_WritesNothingToTheFile()
        {
            var config = TomlConfig.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(RootWithTblArray.Empty, config);

            s.Should().Be(RootWithTblArray.ExpectedEmpty);
        }

        [Fact]
        public void WriteTable_WhenTblArrayValueTypeIsConfiguredAsInlineTable_ThatTypeIsWrittenAsAnInlineTable()
        {
            var config = TomlConfig.Create(cfg =>
                cfg.ConfigureType<Item>(type =>
                    type.TreatAsInlineTable()));

            var s = Toml.WriteString(RootWithTblArray.TwoItems, config);

            s.Should().Be(RootWithTblArray.ExpectedTwoItems);
        }

        public class RootWithDict
        {
            public const string Expected = @"
[Dict]
First = { SVal = ""X"", BVal = true, IVal = 1 }
Second = { SVal = ""Y"", BVal = false, IVal = 2 }
";

            public Dictionary<string, Item> Dict { get; set; } = new Dictionary<string, Item>()
            {
                { "First", new Item() { SVal = "X", BVal = true, IVal = 1 } },
                { "Second", new Item() { SVal = "Y", BVal = false, IVal = 2 }},
            };
        }

        public class RootWithTblArray
        {
            // An empty table array writes nothing to the file
            public const string ExpectedEmpty = @"
";
            public const string ExpectedTwoItems = @"
TblArray = [ { SVal = ""X"", BVal = true, IVal = 1 },
             { SVal = ""Y"", BVal = false, IVal = 2 } ]
";

            public static readonly RootWithTblArray Empty = new RootWithTblArray();
            public static readonly RootWithTblArray TwoItems = new RootWithTblArray()
            {
                TblArray = new List<Item>()
                {
                    new Item() { SVal = "X", BVal = true, IVal = 1 },
                    new Item() { SVal = "Y", BVal = false, IVal = 2 },
                }
            };

            public List<Item> TblArray { get; set; } = new List<Item>();
        }

        public class Item
        {
            public string SVal { get; set; } = "X";
            public bool BVal { get; set; } = true;
            public int IVal { get; set; } = 1;
        }
    }
}
