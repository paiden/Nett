using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Nett.UnitTests.TestUtil;
using Xunit;

namespace Nett.UnitTests
{
    public class WriteCustomDictionaryTests
    {
        [Fact(DisplayName = "Writing complex structure with a custom dictionary converter should write that dictionary as a sub table; tests issue #2")]
        public void WriteCustomDict()
        {
            const string expected = @"
Name = ""Test""

[Dict]
CustomKey1 = ""CustomValue1""
CustomKey2 = ""CustomValue2""

[[AList]]
Buzz = ""buzzbuzz""
[Dict]
CustomKey1 = ""CustomValue1""
CustomKey2 = ""CustomValue2""


[[AList]]
Buzz = ""foofoo""
[Dict]
CustomKey1 = ""CustomValue1""
CustomKey2 = ""CustomValue2""";

            var config = TomlConfig.Create().ConfigureType<CustomDictionary>().As.ConvertTo<TomlTable>().As(
            collection =>
            {
                var table = new TomlTable();
                foreach (var kvp in collection.OrderBy(kvp => kvp.Key))
                {
                    table.Add(kvp.Key, new TomlString(kvp.Value));
                }
                return table;
            }).And.ConvertFrom<TomlTable>().As(table =>
            {
                var collection = new CustomDictionary();
                foreach (var kvp in table.ToDictionary())
                {
                    collection.Add(kvp.Key, kvp.Value as string);
                }
                return collection;
            }).Apply();

            var exampleDict = new CustomDictionary { { "CustomKey1", "CustomValue1" }, { "CustomKey2", "CustomValue2" } };
            var barList = new List<IBar>();
            barList.Add(new Bar("buzzbuzz", exampleDict));
            barList.Add(new Bar("foofoo", exampleDict));
            var test = new Foo("Test", exampleDict, barList);

            string s = Toml.WriteString(test, config);
            s.StripWhitespace().Should().Be(expected.StripWhitespace());
        }

        public class CustomDictionary : Dictionary<string, string>
        {
            public CustomDictionary(IDictionary<string, string> collection) : base(collection)
            {
            }

            public CustomDictionary() : base()
            {
            }
        }

        public interface IBar
        {
            string Buzz { get; }
            CustomDictionary Dict { get; }
        }

        public interface IFoo
        {
            string Name { get; }
            CustomDictionary Dict { get; }
            List<IBar> AList { get; }
        }

        public class Foo : IFoo
        {
            public string Name { get; set; }
            public CustomDictionary Dict { get; set; }
            public List<IBar> AList { get; set; }

            public Foo(string name, CustomDictionary dict, List<IBar> list)
            {
                Name = name;
                Dict = dict;
                AList = list;
            }
        }

        public class Bar : IBar
        {
            public string Buzz { get; set; }
            public CustomDictionary Dict { get; set; }

            public Bar(string buzz, CustomDictionary dict)
            {
                Buzz = buzz;
                Dict = dict;
            }
        }

    }
}
