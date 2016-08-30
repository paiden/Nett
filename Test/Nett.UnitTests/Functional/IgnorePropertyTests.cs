using System;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public class IgnorePropertyTests
    {
        private const int XDefault = 1;
        private const int YDefault = 2;
        private const int ZDefault = 3;

        [Fact(DisplayName = "Writing object with property ignored via attribute, will ignore that property")]
        public void WriteObject_WhenPropertyIgnoredViaAttribute_WillNotWriteThatProperty()
        {
            // Act
            var written = Toml.WriteString(new TableAttributeIgnored());

            // Assert
            var read = Toml.ReadString(written);
            read.Rows.Count.Should().Be(1);
            read.Get<int>("Y").Should().Be(YDefault);
        }

        [Fact(DisplayName = "Writing object with property ignored via fluent API, will not write that property")]
        public void WriteObject_WhenPropertyIgnoredViaFluentApi_WillNotWriteThatAttribute()
        {
            // Arrange
            var config = TomlConfig.Create(cfg => cfg
                .ConfigureType<Table>(tc => tc
                    .IgnoreProperty(i => i.Y)));

            // Act
            var written = Toml.WriteString(new Table(), config);

            // Assert
            var read = Toml.ReadString(written);
            read.Rows.Count.Should().Be(1);
            read.Get<int>("X").Should().Be(XDefault);
        }

        [Fact(DisplayName = "Writing object with sub property ignored via fluent API, will not write that property")]
        public void WriteObject_WhenSubPropertyIgnoredViaFluentApi_WillNotWriteThatAttribute()
        {
            // Arrange
            var config = TomlConfig.Create(cfg => cfg
                .ConfigureType<Table>(tc => tc
                    .IgnoreProperty(i => i.Y)));

            // Act
            var written = Toml.WriteString(new Parent(), config);

            // Assert
            var read = Toml.ReadString(written);
            var subTbl = read.Get<TomlTable>("T");
            subTbl.Rows.Count.Should().Be(1);
            subTbl.Get<int>("X").Should().Be(XDefault);
        }

        [Fact(DisplayName = "Reading object with property ignored via attribute, will ignore that property")]
        public void ReadObject_WhenPropertyIgnoredViaAttribute_WillNotReadThatProperty()
        {
            // Arrange
            const int FileXValue = 5;
            const int FileYValue = 6;
            string src = $@"
X = {FileXValue}
Y = {FileYValue}";

            // Act
            var read = Toml.ReadString<TableAttributeIgnored>(src);

            // Assert
            read.X.Should().Be(XDefault);
            read.Y.Should().Be(FileYValue);
        }

        [Fact(DisplayName = "Reading object with property ignored via fluent API, will ignore that property")]
        public void ReadObject_WhenPropertyIgnoredViaFluentApi_WillNotReadThatProperty()
        {
            // Act
            const int FileXValue = 5;
            const int FileYValue = 6;
            var config = TomlConfig.Create(cfg => cfg
                .ConfigureType<Table>(tc => tc
                    .IgnoreProperty(i => i.X)));
            string src = $@"
X = {FileXValue}
Y = {FileYValue}";
            var read = Toml.ReadString<Table>(src, config);

            // Assert
            read.X.Should().Be(XDefault);
            read.Y.Should().Be(FileYValue);
        }

        public class TableAttributeIgnored
        {
            [TomlIgnore]
            public int X { get; set; } = XDefault;

            public int Y { get; set; } = YDefault;
        }

        public class Table
        {
            public int X { get; set; } = XDefault;
            public int Y { get; set; } = YDefault;
        }

        public class Parent
        {
            public int Z { get; set; } = ZDefault;
            public Table T { get; set; } = new Table();
        }
    }
}
