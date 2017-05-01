using System.Collections.Generic;

namespace Nett.UnitTests.Functional
{
    /// <summary>
    /// Test cases that try to verify all aspects regarding correct handling of inline tables.
    /// </summary>
    public sealed partial class InlineTableTests
    {
        public class InlineArrayAttributeOnItem
        {
            public const string TowItemsSerialized = @"
Items = [ { SVal = ""X"", BVal = true, IVal = 1 },
          { SVal = ""Y"", BVal = false, IVal = 2 } ]
";

            public static readonly InlineArrayAttributeOnItem TwoItems = new InlineArrayAttributeOnItem()
            {
                Items = new List<AttItem>() { AttItem.Item1, AttItem.Item2 }
            };

            public List<AttItem> Items { get; set; } = new List<AttItem>();
        }

        public class InlineDictViaAttribute
        {
            public const string TwoItemsSerialized = @"Dict = { First = { SVal = ""X"", BVal = true, IVal = 1 }, Second = { SVal = ""Y"", BVal = false, IVal = 2 } }
";
            public static InlineDictViaAttribute TwoItems = new InlineDictViaAttribute()
            {
                Dict = new Dictionary<string, Item>()
                {
                    { "First", new Item() { SVal = "X", BVal = true, IVal = 1 } },
                    { "Second", new Item() { SVal = "Y", BVal = false, IVal = 2 }},
                }
            };

            [TomlInlineTable]
            public Dictionary<string, Item> Dict { get; set; } = new Dictionary<string, Item>();
        }

        public class InlineDict
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

        public class InlineArray
        {
            // An empty table array writes nothing to the file
            public const string ExpectedEmpty = @"
";
            public const string ExpectedTwoItems = @"
TblArray = [ { SVal = ""X"", BVal = true, IVal = 1 },
             { SVal = ""Y"", BVal = false, IVal = 2 } ]
";

            public static readonly InlineArray Empty = new InlineArray();
            public static readonly InlineArray TwoItems = new InlineArray()
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

        [TomlInlineTable]
        public class AttItem : Item
        {
            public static readonly AttItem Item1 = new AttItem() { SVal = "X", BVal = true, IVal = 1 };
            public static readonly AttItem Item2 = new AttItem() { SVal = "Y", BVal = false, IVal = 2 };
        }
    }
}
