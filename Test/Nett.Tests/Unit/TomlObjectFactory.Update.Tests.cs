using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public class TomlObjectFactoryUpdateTests
    {
        private TomlTable updateThis;

        public TomlObjectFactoryUpdateTests()
        {
            this.updateThis = Toml.Create();
            this.updateThis.Add("x", 1);
        }

        [Fact]
        public void Update_WhenKeyDoesntExist_ThrowInvalidOperation()
        {
            // Act
            Action a = () => this.updateThis.Update("noexisto", "newvalue");

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage("*noexisto*");
        }

        [Fact]
        public void Update_WhenXSetToNewBoolValue_ChangesRowToThat()
        {
            // Act
            const bool newValue = true;
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<bool>("x").Should().Be(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewStringValue_ChangesRowToThat()
        {
            // Act
            const string newValue = "newvalue";
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<string>("x").Should().Be(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewIntValue_ChangesRowToThat()
        {
            // Act
            this.updateThis.Update("x", 2);

            // Assert
            this.updateThis.Get<int>("x").Should().Be(2);
        }

        [Fact]
        public void Update_WhenXSetToNewFloatValue_ChangesRowToThat()
        {
            // Act
            const double newValue = 2.0;
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<double>("x").Should().Be(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewTimespanValue_ChangesRowToThat()
        {
            // Act
            TimeSpan newValue = TimeSpan.FromSeconds(1);
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<TimeSpan>("x").Should().Be(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewDateTimeValue_ChangesRowToThat()
        {
            // Act
            var newValue = DateTimeOffset.MaxValue;
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<DateTimeOffset>("x").Should().Be(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewBoolArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<bool>() { true, false };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<bool>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewStringArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<string>() { "X", "Y" };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<string>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewLongArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<long>() { 0, 1 };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<long>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewIntArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<int>() { 0, 1 };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<int>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewDoubleArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<double>() { 0, 1 };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<double>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewFloatArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<float>() { 0, 1 };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<float>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewDateTimeOffsetArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<DateTimeOffset>() { DateTimeOffset.MinValue, DateTimeOffset.MaxValue };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<DateTimeOffset>>("x").Should().Equal(newValue);
        }

        [Fact(Skip = "There seems to be a bug with datetime. Enable if datetime issue is fixed / validated")]
        public void Update_WhenXSetToNewDateTimeArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<DateTime>() { new DateTime(2000, 1, 1), new DateTime(2000, 2, 1) };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<DateTime>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewTimespanArray_ChangesRowToThat()
        {
            // Act
            var newValue = new List<TimeSpan>() { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<List<TimeSpan>>("x").Should().Equal(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewObject_ChangesRowToThat()
        {
            // Act
            var newValue = new Foo();
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<Foo>("x").Should().Be(newValue);
        }

        [Fact]
        public void Update_WhenXSetToNewValueObject_ChangesRowToThat()
        {
            // Act
            var newValue = new FooStruct() { X = 1 };
            this.updateThis.Update("x", newValue);

            // Assert
            this.updateThis.Get<FooStruct>("x").Should().Be(newValue);
        }

        private struct FooStruct
        {
            private int x;
            public int X { get => this.x; set => this.x = value; }
        }

        private class Foo
        {
            public int X { get; set; } = 1;

            public override bool Equals(object obj) => ((Foo)obj).X == this.X;

            public override int GetHashCode() => this.X;
        }
    }
}
