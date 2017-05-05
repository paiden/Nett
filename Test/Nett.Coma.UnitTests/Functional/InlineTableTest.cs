using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Nett.UnitTests.Util;
using Xunit;

namespace Nett.Coma.Tests.Functional
{
    public sealed class InlineTableTest
    {
        public class Cfg
        {
            [TreatAsInlineTable]
            public class Items : Dictionary<string, bool> { };

            public Items UserItems { get; set; }
        }

        [Fact]
        public void Write_WithInlineTableProperty_WritesThatTableAsInlineTable()
        {
            using (var file = TestFileName.Create(nameof(Write_WithInlineTableProperty_WritesThatTableAsInlineTable), "Input", ".toml"))
            {
                // Arrange
                const string expected = @"UserItems = { X = true, Y = false }
";
                const string input = "UserItems = { 'X' = false, 'Y' = true }";
                File.WriteAllText(file, input);
                var toWrite = new Cfg.Items()
                {
                    { "X", true },
                    { "Y", false }
                };

                // Act
                var cfg = Config.Create(() => new Cfg(), ConfigSource.CreateFileSource(file));
                cfg.Set(c => c.UserItems, toWrite);

                // Assert
                var f = File.ReadAllText(file);
                f.Should().Be(expected);
            }
        }
    }
}
