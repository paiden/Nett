using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Internal
{
    public sealed class TomlTableToTypedDictionaryConverterTests
    {
        [Fact]
        public void CanConvertTo_ForDerived_IsTrue()
        {
            var c = Create();

            bool r = c.CanConvertTo(typeof(Derived));

            r.Should().Be(true);
        }

        [Fact]
        public void Convert_WithDerivedType_CanConvertCorrectInMemoryDictionary()
        {
            // Arrange
            var tbl = CreateInputTable();
            var c = Create();

            // Act
            var r = (Derived)c.Convert(tbl.Root, tbl, typeof(Derived));

            // Assert
            AssertReadDictIsCorrect(r);
        }

        [Fact]
        public void Convert_WithDerivedAgainType_CanConvertCorrectInMemoryDictionary()
        {
            // Arrange
            var tbl = CreateInputTable();
            var c = Create();

            // Act
            var r = (DerivedAgain)c.Convert(tbl.Root, tbl, typeof(DerivedAgain));

            // Assert
            AssertReadDictIsCorrect(r); ;
        }

        [Fact]
        public void Convert_WithDerivedWithGenericArgsType_CanConvertCorrectInMemoryDictionary()
        {
            // Arrange
            var tbl = CreateInputTable();
            var c = Create();

            // Act
            var r = (DerivedWithGenericArgs<bool, string>)c.Convert(tbl.Root, tbl, typeof(DerivedWithGenericArgs<bool, string>));

            // Assert
            AssertReadDictIsCorrect(r);
        }

        private static TomlTable CreateInputTable()
        {
            var tbl = Toml.Create();
            tbl.Add("X", 1.0);
            tbl.Add("Y", 0.0);
            return tbl;
        }

        private static void AssertReadDictIsCorrect(Dictionary<string, double> d)
        {
            d.Count.Should().Be(2);
            d["X"].Should().Be(1.0);
            d["Y"].Should().Be(0.0);
        }

        private static TomlTableToTypedDictionaryConverter Create()
        {
            return new TomlTableToTypedDictionaryConverter();
        }

        public class Derived : Dictionary<string, double> { };

        public class DerivedAgain : Derived { };

        public class DerivedWithGenericArgs<K, V> : Derived { }
    }
}
