using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests.Issues
{
    public sealed class Issue74Tests
    {
        [Fact]
        public void GivenTomlNotContainingAnyTableForItems_WhenGettingWithDefaultValue_DefaultParamIsReturnedInstadOfThrowingKeyNotFound()
        {
            // Arrange
            File.WriteAllText("test.toml",
                @"");
            var cfg = Config.CreateAs()
                .MappedToType(() => new Cfg())
                .StoredAs(store => store
                    .File("test.toml"))
                .Initialize();

            // Act
            var items = cfg.Get(c => c.Items, new CfgItem[0]);

            // Assert
            items.Should().BeEmpty();
        }

        [Fact]
        public void GivenTomlNotContainingAnyTableForItems_WhenGettingWithoutDefaultValue_ThrowsKeyNotFound()
        {
            // Arrange
            File.WriteAllText("test.toml",
                @"");
            var cfg = Config.CreateAs()
                .MappedToType(() => new Cfg())
                .StoredAs(store => store
                    .File("test.toml"))
                .Initialize();

            // Act
            Action a = () => cfg.Get(c => c.Items);

            // Assert
            a.ShouldThrow<KeyNotFoundException>();
        }

        public class Cfg
        {
            public CfgItem[] Items { get; set; } = new CfgItem[0];
        }

        public class CfgItem
        {
            public string Key { get; set; } = "def";
        }
    }
}
