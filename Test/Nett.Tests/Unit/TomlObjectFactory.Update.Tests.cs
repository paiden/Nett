using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed partial class TomlObjectFactoryTests
    {
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

        [Fact]
        public void Update_WhenXSetToNewDateTimeArray_ChangesRowToThat()
        {
            // Act
            long ticks = new DateTime(2000, 1, 1).Ticks;
            var newValue = new List<DateTime>() { new DateTime(ticks, DateTimeKind.Utc), new DateTime(ticks + 1, DateTimeKind.Utc) };
            this.updateThis.Update("x", newValue);

            // Assert
            var val = this.updateThis.Get<List<DateTime>>("x");
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
        public void Update_WithFooClassListSource_UpdatesToTableArray()
        {
            // Act
            var ta = this.updateThis.Update("x", FooClassList.List1);

            // Assert
            FooClassList.List1.AssertIs(this.updateThis["x"]);
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

        [Fact]
        public void Update_WithFooClass_UpdatesToTable()
        {
            // Act
            this.updateThis.Update("x", FooClass.Foo1);

            // Assert
            FooClass.Foo1.AssertIs(this.updateThis["x"]);
        }

        [Fact]
        public void Update_WithFooStruct_UpdatesToTable()
        {
            // Act
            this.updateThis.Update("x", FooStruct.Foo1);

            // Assert
            FooStruct.Foo1.AssertIs(this.updateThis["x"]);
        }

        [Fact]
        public void Update_WithFooDict_UpdatesToTable()
        {
            // Act
            var u = this.updateThis.Update("x", FooDict.Dict1);

            // Assert
            FooDict.Dict1.AssertIs(this.updateThis["x"]);
            FooDict.Dict1.AssertIs(u);
        }

        [Fact]
        public void Update_WithFooClassList_UpdatesToTableArray()
        {
            // Act
            var ta = this.updateThis.Update("x", FooClassList.List1);

            // Assert
            FooClassList.List1.AssertIs(this.updateThis["x"]);
        }

        [Fact]
        public void Update_WithFooStructList_UpdatesToTableArray()
        {
            // Act
            var ta = this.updateThis.Update("x", FooStructList.List1);

            // Assert
            FooStructList.List1.AssertIs(this.updateThis["x"]);
        }
    }
}
