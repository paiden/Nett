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
    }
}
