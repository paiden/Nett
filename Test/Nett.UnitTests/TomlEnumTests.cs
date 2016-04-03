using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class TomlEnumTests
    {
        private enum TestEnum
        {
            DefaultValue = 0,
            SomeValue = 10,
            AnotherValue = 50
        }

        [Flags]
        private enum TestFlags
        {
            NoFlags = 0,
            FlagA = 1,
            FlagB = 2,
            FlagC = 4,
            FlagD = 8
        }

        [Fact]
        public void SerializeAndDeserializeEnum_WorksCorrectly()
        {
            string tomlStringTable = 
                  "SomeKey = \"SomeValue\"" + Environment.NewLine
                + "AnotherKey = \"AnotherValue\"" + Environment.NewLine
                + "DefaultKey = \"DefaultValue\"" + Environment.NewLine;

            // Deserialize
            TomlTable table = Toml.ReadString(tomlStringTable);

            TestEnum defaultValue = table.Get<TestEnum>("DefaultKey");
            Assert.Equal(TestEnum.DefaultValue, defaultValue);

            TestEnum anotherValue = table.Get<TestEnum>("AnotherKey");
            Assert.Equal(TestEnum.AnotherValue, anotherValue);

            TestEnum someValue = table.Get<TestEnum>("SomeKey");
            Assert.Equal(TestEnum.SomeValue, someValue);

            // Serialize
            // should not throw any exceptions
            string value = Toml.WriteString(table);

            Assert.True(tomlStringTable.Equals(value, StringComparison.Ordinal));
        }

        [Fact]
        public void SerializeAndDeserializeFlagEnum_WorksCorrectly()
        {
            // TODO: no flags test
            string tomlStringTable =
                  "FlagAKey = \"FlagA\"" + Environment.NewLine                 
                + "FlagABKey = \"FlagA, FlagB\"" + Environment.NewLine
                + "FlagABCDKey = \"FlagA, FlagB, FlagC, FlagD\"" + Environment.NewLine;

            TomlTable table = Toml.ReadString(tomlStringTable);

            TestFlags flagA = table.Get<TestFlags>("FlagAKey");
            Assert.Equal(TestFlags.FlagA, flagA);

            TestFlags flagAB = table.Get<TestFlags>("FlagABKey");
            Assert.Equal(TestFlags.FlagA | TestFlags.FlagB, flagAB);

            TestFlags flagABCD = table.Get<TestFlags>("FlagABCDKey");
            Assert.Equal(TestFlags.FlagA | TestFlags.FlagB | TestFlags.FlagC | TestFlags.FlagD, flagABCD);

            // Serialize
            // should not throw any exceptions
            string value = Toml.WriteString(table);

            Assert.True(tomlStringTable.Equals(value, StringComparison.Ordinal));
        }
    }
}
