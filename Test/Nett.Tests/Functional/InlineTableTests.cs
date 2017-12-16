using System.Collections.Generic;

namespace Nett.Tests.Functional
{
    /// <summary>
    /// Test cases that try to verify all aspects regarding correct handling of inline tables.
    /// </summary>
    public sealed partial class InlineTableTests
    {
        public class ConfigWithStringBoolDict
        {
            public const string Serialized = @"
[User]
Items = { X = true, Y = false }
";

            [TreatAsInlineTable]
            public class Items : Dictionary<string, bool> { };

            public UserSettings User { get; set; } = new UserSettings();

            public class UserSettings
            {
                public Items Items { get; set; } = new Items()
            {
                { "X", true },
                { "Y", false },
            };
            }
        }

        public class InlineArrayAttributeOnItem
        {
            public const string TowItemsSerialized = @"Items = [ { SVal = ""X"", BVal = true, IVal = 1 },
          { SVal = ""Y"", BVal = false, IVal = 2 } ]

";

            public static readonly InlineArrayAttributeOnItem TwoItems = new InlineArrayAttributeOnItem()
            {
                Items = new List<AttItem>() { AttItem.AttItem1, AttItem.AttItem2 }
            };

            public List<AttItem> Items { get; set; } = new List<AttItem>();
        }

        public class ItemDict
        {
            public const string TwoItemsInlineSerialzed = @"
[Dict]
First = { SVal = ""X"", BVal = true, IVal = 1 }
Second = { SVal = ""Y"", BVal = false, IVal = 2 }
";
            public const string TwoItemsDictInlineSerialized = @"Dict = { First = { SVal = ""X"", BVal = true, IVal = 1 }, Second = { SVal = ""Y"", BVal = false, IVal = 2 } }
";


            public static readonly ItemDict TwoItems = new ItemDict()
            {
                Dict = new Dictionary<string, Item>()
                {
                    { "First", Item.Item1 },
                    { "Second", Item.Item2 },
                }
            };

            public Dictionary<string, Item> Dict { get; set; } = new Dictionary<string, Item>();
        }

        public class InlineArray
        {
            // An empty table array writes nothing to the file
            public const string ExpectedEmpty = @"

";
            public const string ExpectedTwoItems = @"TblArray = [ { SVal = ""X"", BVal = true, IVal = 1 },
             { SVal = ""Y"", BVal = false, IVal = 2 } ]

";

            public static readonly InlineArray Empty = new InlineArray();
            public static readonly InlineArray TwoItems = new InlineArray()
            {
                TblArray = new List<Item>() { Item.Item1, Item.Item2 }
            };

            public List<Item> TblArray { get; set; } = new List<Item>();
        }

        public class Item
        {
            public static readonly AttItem Item1 = new AttItem() { SVal = "X", BVal = true, IVal = 1 };
            public static readonly AttItem Item2 = new AttItem() { SVal = "Y", BVal = false, IVal = 2 };

            public string SVal { get; set; } = "X";
            public bool BVal { get; set; } = true;
            public int IVal { get; set; } = 1;
        }

        [TreatAsInlineTable]
        public class AttItem : Item
        {
            public static readonly AttItem AttItem1 = new AttItem() { SVal = "X", BVal = true, IVal = 1 };
            public static readonly AttItem AttItem2 = new AttItem() { SVal = "Y", BVal = false, IVal = 2 };
        }
    }
}
