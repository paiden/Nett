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
            read.Rows.Count.Should().Be(0);
            read.Get<int>("X").Should().Be(1);
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

        [Fact(DisplayName = "Writing object with sub property ignored via attribute, will ignore that property")]
        public void WriteObject_WhenSubPropertyIgnoredViaAttribute_WillNotWriteThatProperty()
        {
            // Act
            var written = Toml.WriteString(new TableAttributeIgnored());

            // Assert
            var read = Toml.ReadString(written);
            read.Rows.Count.Should().Be(0);
            read.Get<int>("X").Should().Be(XDefault);

            throw new NotImplementedException();
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
            // Act
            const string src = @"
X = 5
Y = 6";
            var read = Toml.ReadString(src);

            // Assert
            read.Get<int>("X").Should().Be(XDefault);
            read.Get<int>("Y").Should().Be(6);
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
            public int X { get; set; } = XDefault;

            [TomlIgnore]
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
