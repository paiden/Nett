using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.AspNet.Tests
{
    public sealed class ProviderDictionaryConverterTests
    {
        [Fact]
        public void ToProviderDictionary_CreatesDictionaryCorrespondingToTomlContent()
        {
            // Arrange
            var tml = Toml.ReadString(@"
Option1 = ""Hello From TOML""
Option2 = 666

[subsection]
SubOption1 = ""SubHello from TOML""
SubOption2 = 333

[subsection.SubSubOpts]
SSOPT1 = ""hello""
SSOPT2 = 16.4");

            // Act
            var providerDict = ProviderDictionaryConverter.ToProviderDictionary(tml);

            // Assert
            providerDict.Should().Equal(new Dictionary<string, string>()
            {
                { "Option1", "Hello From TOML" },
                { "Option2", "666" },
                { "subsection:SubOption1", "SubHello from TOML" },
                { "subsection:SubOption2", "333" },
                { "subsection:SubSubOpts:SSOPT1", "hello" },
                { "subsection:SubSubOpts:SSOPT2", "16.4" },
            });
        }

        [Fact]
        public void ToProviderDictionary_Converts_TomlTableArray()
        {
            // Arrange
            var tml = Toml.ReadString(@"
[[tableArray]]

[[tableArray]]
STA1 = ""array1""
STA2 = 1

[[tableArray]]

[[tableArray]]
STA1 = ""array3""
STA2 = 3

[[tableArray.nestedTableArray]]
NTA1 = ""nestedArray0""
NTA2 = 10

[[tableArray.nestedTableArray]]

[[tableArray.nestedTableArray]]
NTA1 = ""nestedArray2""
NTA2 = 22");

            // Act
            var providerDict = ProviderDictionaryConverter.ToProviderDictionary(tml);

            // Assert
            providerDict.Should().Equal(new Dictionary<string, string>()
            {
                { "tableArray:1:STA1", "array1" },
                { "tableArray:1:STA2", "1" },
                { "tableArray:3:STA1", "array3" },
                { "tableArray:3:STA2", "3" },
                { "tableArray:3:nestedTableArray:0:NTA1", "nestedArray0" },
                { "tableArray:3:nestedTableArray:0:NTA2", "10" },
                { "tableArray:3:nestedTableArray:2:NTA1", "nestedArray2" },
                { "tableArray:3:nestedTableArray:2:NTA2", "22" },
            });
        }

        [Fact]
        public void ToProviderDictionary_Converts_JaggedArray()
        {
            // Arrange
            var tml = Toml.ReadString(@"
jaggedArray = [ [ ""index00"", ""index01"" ], [ ""index10"" ], [], [ ""index30"" ] ]
");

            // Act
            var providerDict = ProviderDictionaryConverter.ToProviderDictionary(tml);

            // Assert
            providerDict.Should().Equal(new Dictionary<string, string>()
            {
                { "jaggedArray:0:0", "index00" },
                { "jaggedArray:0:1", "index01" },
                { "jaggedArray:1:0", "index10" },
                { "jaggedArray:3:0", "index30" }
            });
        }
    }
}
