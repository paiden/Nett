using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.AspNet.Tests
{
    public sealed class VerifyIssueTests
    {
        [Fact]
        public void VerifyIssue71_WithDeepNestedObjects_StillProdcuesCorrectProviderDict()
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
        public void VerifyIssue71_TomlWithTableArray_ProdcuesCorrectProviderDict()
        {
            // Arrange
            var tml = Toml.ReadString(@"
Option1 = ""Hello From TOML""
Option2 = 666

A = [1.2, 3.4]

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
                { "A:0", "1.2" },
                { "A:1", "3.4" },
                { "subsection:SubOption1", "SubHello from TOML" },
                { "subsection:SubOption2", "333" },
                { "subsection:SubSubOpts:SSOPT1", "hello" },
                { "subsection:SubSubOpts:SSOPT2", "16.4" },
            });
        }
    }
}
