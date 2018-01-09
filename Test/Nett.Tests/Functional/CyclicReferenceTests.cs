using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Functional
{
    [ExcludeFromCodeCoverage]
    public sealed class CyclicReferenceTests
    {
        private class Container
        {
            public Parent Item { get; set; }
        }

        private class Parent
        {
            public Child Child { get; set; }
        }

        private class Child
        {
            public Parent Parent { get; set; }
        }

        [Fact]
        public void CreateTomlTable_WhenClrObjectContainsCircRef_ThrowsNonStackOverflowException()
        {
            // Arrange
            var subject = new Parent();
            subject.Child = new Child { Parent = subject };

            // Act
            Action a = () => Toml.Create(subject);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage("*Cyclic*");
        }

        [Fact]
        public void CreateTomlTable_WhenCyclicRefErrorIsThrown_ContainsInformationAboutTheProrpertyCausingTheError()
        {
            // Arrange
            var subject = new Parent();
            subject.Child = new Child { Parent = subject };

            // Act
            Action a = () => Toml.Create(subject);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage(
                "A circular reference was detected for property 'Parent' of Type " +
                "'Nett.Tests.Functional.CyclicReferenceTests+Child' using path 'Child'.");
        }

        [Fact]
        public void CreateTomlTable_WhenCyclicRefErrorIsThrown_ContainsPathInformation()
        {
            // Arrange
            var subject = new Parent();
            subject.Child = new Child { Parent = subject };
            var container = new Container() { Item = subject };

            // Act
            Action a = () => Toml.Create(container);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage(
                "A circular reference was detected for property 'Parent' of Type " +
                "'Nett.Tests.Functional.CyclicReferenceTests+Child' using path 'Item.Child'.");
        }

        [Fact]
        public void CreateTomlTable_WhenDictionaryContainsCircRef_ThrowsNonStackOverflowException()
        {
            // Arrange
            var subject = new Dictionary<string, object>();
            var child = new Dictionary<string, object>();

            subject.Add("child", child);
            child.Add("parent", subject);

            // Act
            Action a = () => Toml.Create(subject);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage(
                "A circular reference was detected for key 'parent' using path 'child'.");
        }

        [Fact]
        public void CreateTomlTable_WhenDictionaryContainsCircRef_ContainsPath()
        {
            // Arrange
            var container = new Dictionary<string, object>();
            var subject = new Dictionary<string, object>();
            var child = new Dictionary<string, object>();

            container.Add("item", subject);
            subject.Add("child", child);
            child.Add("parent", subject);

            // Act
            Action a = () => Toml.Create(container);

            // Assert
            a.ShouldThrow<InvalidOperationException>().WithMessage(
                "A circular reference was detected for key 'parent' using path 'item.child'.");
        }
    }
}
