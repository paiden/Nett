using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Functional
{
    [ExcludeFromCodeCoverage]
    public sealed class CyclicReferenceTests
    {
        private class Parent
        {
            public Child Child { get; set; }
        }

        private class Child
        {
            public Parent Parent { get; set; }
        }

        [Fact]
        public void CreateTomlTable_WhenClrObjectContainsCircularReference_ThrowsNonStackOverflowException()
        {
            // Arrange
            var subject = new Parent();
            subject.Child = new Child { Parent = subject };

            // Act
            Action a = () => Toml.Create(subject);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage("*Cyclic*");
        }
    }
}
