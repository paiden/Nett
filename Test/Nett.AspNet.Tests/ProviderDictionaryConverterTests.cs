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
        public void ToProviderDictionary_WhenTomlContainsTblArray_ProducesUsefulErrorMessage()
        {
            // Arrange
            var tml = Toml.ReadString(@"
[[x]]
[[x]]");

            // Act
            Action a = () => ProviderDictionaryConverter.ToProviderDictionary(tml);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage("AspNet provider cannot handle TOML object of type 'array of tables'. The objects key is 'x'.");
        }

        [Fact]
        public void ToProviderDictionary_WhenTomlContainsJaggedArray_ProducesUsefulErrorMessage()
        {
            // Arrange
            var tml = Toml.ReadString(@"
x = [[1]]");

            // Act
            Action a = () => ProviderDictionaryConverter.ToProviderDictionary(tml);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage("AspNet provider cannot handle jagged arrays, only simple arrays are supported.The arrays key is 'x'.");
        }
    }
}
