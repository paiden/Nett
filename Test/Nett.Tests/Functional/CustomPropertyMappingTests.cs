using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests.Functional
{
    public sealed class CustomPropertyMappingTests
    {
        [Fact]
        public void Read_WhenSelectorRuleIsUsedAndPropMatchesExactly_MapsProperty()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseTargetPropertySelector(standardSelectors => standardSelectors.Exact)));

            // Act
            var read = Toml.ReadString<TestObject>("TestProp='x'", settings);

            // Assert
            read.TestProp.Should().Be("x");
        }

        [Fact]
        public void Read_WhenExactSelectorIsUsedAndKeyNameDiffers_PropertyIsClrDefaultInsteadOfFileValue()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseTargetPropertySelector(standardSelectors => standardSelectors.Exact)));

            // Act
            var read = Toml.ReadString<TestObject>("testprop='x'", settings);

            // Assert
            read.TestProp.Should().BeNull();
        }

        [Fact]
        public void Read_WhenIgoreCaseSelectorIsUsedAndKeyNameMatches_MapsProperty()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseTargetPropertySelector(standardSelectors => standardSelectors.IgnoreCase)));

            // Act
            var read = Toml.ReadString<TestObject>("TestProp='x'", settings);

            // Assert
            read.TestProp.Should().Be("x");
        }

        [Fact]
        public void Read_WhenIgoreCaseSelectorIsUsedAndKeyDiffers_MapsProperty()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseTargetPropertySelector(standardSelectors => standardSelectors.IgnoreCase)));

            // Act
            var read = Toml.ReadString<TestObject>("tESTpROP='x'", settings);

            // Assert
            read.TestProp.Should().Be("x");
        }

        [Fact]
        public void Write_WhenPropNameGenIsUsed_UsesPropertyNameAsTomlKey()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseKeyGenerator(standardGenerators => standardGenerators.PropertyName)));

            // Act
            var written = Toml.WriteString(Create("x"), settings);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("TestProp=\"x\"");
        }

        [Fact]
        public void Write_WhenUpperCaseGeneratorUsed_WritesUpperCaseKey()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseKeyGenerator(standardGenerators => standardGenerators.UpperCase)));

            // Act
            var written = Toml.WriteString(Create("x"), settings);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("TESTPROP=\"x\"");
        }


        [Fact]
        public void Write_WhenLowerCaseGeneratorUsed_WritesLowerCaseKey()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

            // Act
            var written = Toml.WriteString(Create("x"), settings);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("testprop=\"x\"");
        }

        [Fact]
        public void Write_WhenCamelCaseGeneratorUsed_WritesCamelCaseKey()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseKeyGenerator(standardGenerators => standardGenerators.CamelCase)));

            // Act
            var written = Toml.WriteString(new CCTestObject() { testProp = "x" }, settings);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("TestProp=\"x\"");
        }

        [Fact]
        public void Write_WhenPascalCaseGeneratorUsed_WritesPascalCaseKey()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseKeyGenerator(standardGenerators => standardGenerators.PascalCase)));

            // Act
            var written = Toml.WriteString(new CCTestObject() { testProp = "x" }, settings);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("testProp=\"x\"");
        }

        [Fact]
        public void Write_WhenLowerCaseGenUsedOnNestedObject_WritesCorrectToml()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

            // Act
            var written = Toml.WriteString(new RootTestObject(), settings);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo("prop=1[sub]subprop=2");
        }

        [Fact]
        public void Read_WhenIgnoreCaseSelectorUsedOnNestedOjbect_CreatesCorrectDataObject()
        {
            // Arrange
            var settings = TomlSettings.Create(s => s
                .ConfigurePropertyMapping(m => m
                    .UseTargetPropertySelector(standardSelectors => standardSelectors.IgnoreCase)));

            // Act
            var read = Toml.ReadString<RootTestObject>(@"
prop=3
[suB]
subprop=4", settings);

            // Assert
            read.Prop.Should().Be(3);
            read.Sub.SubProp.Should().Be(4);
        }

        private static TestObject Create(string value)
            => new TestObject() { TestProp = value };

        public class CCTestObject
        {
            public string testProp { get; set; }
        }

        public class TestObject
        {
            public string TestProp { get; set; }
        }

        public class RootTestObject
        {
            public int Prop { get; set; } = 1;


            public SubTestObject Sub { get; set; } = new SubTestObject();

            public class SubTestObject
            {
                public int SubProp { get; set; } = 2;
            }
        }
    }
}
