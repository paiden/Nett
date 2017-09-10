using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Internal
{
    //TODO: Move to public API test as soon as API is adapted for new config mechanism
    [ExcludeFromCodeCoverage]
    public class NestedTableViaConverterTests
    {
        private const string SerializedTomlForDictconverter = @"
Name = ""Test""

[Dict]
CustomKey1 = ""CustomValue1""
CustomKey2 = ""CustomValue2""

[[AList]]
Buzz = ""buzzbuzz""
[AList.Dict]
CustomKey1 = ""CustomValue1""
CustomKey2 = ""CustomValue2""


[[AList]]
Buzz = ""foofoo""
[AList.Dict]
CustomKey1 = ""CustomValue1""
CustomKey2 = ""CustomValue2""";

        private readonly TomlSettings config = TomlSettings.Create(cfg => cfg
            .ConfigureType<CustomDictionary>(ct => ct
                .WithConversionFor<TomlTable>(conv => conv
                    .ToToml((CustomDictionary collection, TomlTable targetTable) =>
                    {
                        foreach (var kvp in collection.OrderBy(kvp => kvp.Key))
                        {
                            targetTable.Add(kvp.Key, kvp.Value);
                        }
                    })
                    .FromToml((metaData, table) =>
                    {
                        var collection = new CustomDictionary();
                        foreach (var kvp in table.ToDictionary())
                        {
                            collection.Add(kvp.Key, kvp.Value as string);
                        }
                        return collection;
                    })
                )
            )
            .ConfigureType<IBar>(ct => ct
                .CreateInstance(() => new Bar())
            )
        );

        [Fact(DisplayName = "Writing complex structure with a custom dictionary converter should write that dictionary as a sub table; tests issue #2")]
        public void Write_WithConverterThatTransformsDictToTable_ProducesValidTomlContent()
        {
            var exampleDict = new CustomDictionary { { "CustomKey1", "CustomValue1" }, { "CustomKey2", "CustomValue2" } };
            var barList = new List<IBar>();
            barList.Add(new Bar("buzzbuzz", exampleDict));
            barList.Add(new Bar("foofoo", exampleDict));
            var test = new Foo("Test", exampleDict, barList);

            string s = Toml.WriteString(test, this.config);
            s.ShouldBeSemanticallyEquivalentTo(SerializedTomlForDictconverter);
        }

        [Fact(DisplayName = "Reading TOML content a dictionary converter is registered for should produce the correct in memory data structure")]
        public void Read_WithConverterThatTransformsTableToDict_ProducesValidInMemoryStructure()
        {
            // Act
            var f = Toml.ReadString<Foo>(SerializedTomlForDictconverter, this.config);

            // Assert
            f.Name.Should().Be("Test");
            f.Dict.Keys.Count.Should().Be(2);
            f.Dict["CustomKey1"].Should().Be("CustomValue1");
            f.Dict["CustomKey2"].Should().Be("CustomValue2");

            f.AList.Count.Should().Be(2);
            f.AList[0].Buzz.Should().Be("buzzbuzz");
            f.AList[0].Dict["CustomKey1"].Should().Be("CustomValue1");
            f.AList[0].Dict["CustomKey2"].Should().Be("CustomValue2");

            f.AList[1].Buzz.Should().Be("foofoo");
            f.AList[1].Dict["CustomKey1"].Should().Be("CustomValue1");
            f.AList[1].Dict["CustomKey2"].Should().Be("CustomValue2");
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

            public Foo()
            {

            }

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

            public Bar()
            {

            }

            public Bar(string buzz, CustomDictionary dict)
            {
                Buzz = buzz;
                Dict = dict;
            }
        }

    }
}
