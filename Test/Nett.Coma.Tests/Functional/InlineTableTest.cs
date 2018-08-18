using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Nett.Tests.Util;
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
            using (var file = TestFileName.Create("Input", ".toml"))
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
                var cfg = Config.CreateAs()
                    .MappedToType(() => new Cfg())
                    .StoredAs(store => store.File(file))
                    .Initialize();
                cfg.Set(c => c.UserItems, toWrite);

                // Assert
                var f = File.ReadAllText(file);
                f.ShouldBeNormalizedEqualTo(expected);
            }
        }

        [Fact]
        public void Read_WithInlineTableProperty_ReadsThatPropertyCorrectly()
        {
            using (var file = TestFileName.Create("input", ".toml"))
            {
                // Arrange
                const string input = "UserItems = { 'X' = false, 'Y' = true }";
                File.WriteAllText(file, input);

                // Act
                var cfg = Config.CreateAs()
                    .MappedToType(() => new Cfg())
                    .StoredAs(store => store.File(file))
                    .Initialize();
                var items = cfg.Get(c => c.UserItems);

                // Assert
                items.Count.Should().Be(2);
                items["X"].Should().Be(false);
                items["Y"].Should().Be(true);
            }
        }
    }
}
