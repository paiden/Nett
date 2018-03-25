using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public sealed class TableArrayTests
    {
        [Fact(DisplayName = "Table array with inline tables is read correctly")]
        public void ReadTableArray_WithInlineTables_ReadsThemCorrectly()
        {
            // Arrange
            const string tml = @"
points = [ { x = 1, y = 2, z = 3 },
           { x = 7, y = 8, z = 9 },
           { x = 2, y = 4, z = 8 } ]
";

            // Act
            var read = Toml.ReadString(tml);

            // Assert
            read["points"].GetType().Should().Be(typeof(TomlTableArray));
            read.Get<TomlTableArray>("points").Count.Should().Be(3);
        }

        [Fact(DisplayName = "Table array with inline tables and beginning/ending newlines is read correctly")]
        public void ReadTableArray_WithInlineTables_Style2_ReadsThemCorrectly()
        {
            // Arrange
            const string tml = @"
points = [
    { x = 1, y = 2, z = 3 },
    { x = 7, y = 8, z = 9 },
    { x = 2, y = 4, z = 8 }
]
";

            // Act
            var read = Toml.ReadString(tml);

            // Assert
            read["points"].GetType().Should().Be(typeof(TomlTableArray));
            read.Get<TomlTableArray>("points").Count.Should().Be(3);
        }

        [Fact(DisplayName = "Table array with inline tables and external specified table is read correctly")]
        public void ReadTableArray_WithSecondTable_ReadsThemCorrectly()
        {
            // Arrange
            const string tml = @"
points = [ { x = 1, y = 2, z = 3 },
           { x = 7, y = 8, z = 9 },
           { x = 2, y = 4, z = 8 } ]
[[points]]
x = 12
y = 42
z = 35
";

            // Act
            var read = Toml.ReadString(tml);

            // Assert
            read["points"].GetType().Should().Be(typeof(TomlTableArray));
            read.Get<TomlTableArray>("points").Count.Should().Be(4);
        }

        [Fact(DisplayName = "Table array with inline tables and external specified inline table cannot be read because of type mixture")]
        public void ReadTableArray_WithSecondInlineTable_Throws()
        {
            // Arrange
            const string tml = @"
points = [ { x = 1, y = 2, z = 3 },
           { x = 7, y = 8, z = 9 },
           { x = 2, y = 4, z = 8 } ]
points =  { x = 12, y = 42, z = 35 };
";

            // Act
            Action a = () => Toml.ReadString(tml);

            // Assert
            a.ShouldThrow<ArgumentException>();
        }

        /// <summary>
        /// This is a corner case where the TOML spec is very vague. For now I will not allow it as it would make the
        /// parser logic somewhat weird (during parsing the newly read item would get merged into the old one without using
        /// the explicitly dedicated [[x]] syntax for it.)
        /// </summary>
        [Fact(DisplayName = "Table array with inline tables and external specified inline table array cannot be read")]
        public void ReadTableArray_WithSecondInlineTableArray_ReadsThemCorrectly()
        {
            // Arrange
            const string tml = @"
points = [ { x = 1, y = 2, z = 3 },
           { x = 7, y = 8, z = 9 },
           { x = 2, y = 4, z = 8 } ]
points = [ { x = 12, y = 42, z = 35 }];
";

            // Act
            Action a = () => Toml.ReadString(tml);

            // Assert
            a.ShouldThrow<ArgumentException>();
        }
    }
}
