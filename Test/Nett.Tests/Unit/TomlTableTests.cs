using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Nett.Coma.Test.Util;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class TomlTableTests
    {
        public TomlTableTests()
        { 
        }

        [MFact(nameof(TomlTable), nameof(TomlTable.Freeze), "Freezes current table")]
        public void Freeze_FreezesCurrentTable()
        {
            using (var scenario = MultiLevelTableScenario.Setup())
            {
                // Act
                scenario.Table.Freeze();

                // Assert
                Action a = () => scenario.Table.Add("new", 0.0);
                a.ShouldThrow<InvalidOperationException>().WithMessage("*frozen*");
            }
        }

        [MFact(nameof(TomlTable), nameof(TomlTable.Freeze), "Freezes table recursive")]
        public void Freeze_FreezesRecursive()
        {
            using (var scenario = MultiLevelTableScenario.Setup())
            {
                // Act
                scenario.Table.Freeze();

                // Assert
                Action a = () => scenario.SubTable.Add("new", (long)0);
                a.ShouldThrow<InvalidOperationException>().WithMessage("*frozen*");
            }
        }

        [MFact(nameof(TomlTable), nameof(TomlTable.Freeze), "When table was not frozen yet, true is returned")]
        public void Freeze_WhenTableNotFrozenYet_ReturnsTrue()
        {
            using (var scenario = MultiLevelTableScenario.Setup())
            {
                // Act
                var r = scenario.Table.Freeze();

                // Assert
                r.Should().Be(true);
            }
        }

        [MFact(nameof(TomlTable), nameof(TomlTable.Freeze), "When table was frozen already, false is returned")]
        public void Freeze_WhenTableWasFrozenAlready_ReturnsFalse()
        {
            using (var scenario = MultiLevelTableScenario.Setup())
            {
                // Arrange
                scenario.Table.Freeze();

                // Act
                var r = scenario.Table.Freeze();

                // Assert
                r.Should().Be(false);
            }
        }

        public static IEnumerable<object[]> OperationsThatFailWhenFrozen
        {
            get
            {
                yield return new object[] { "Add", (Action<TomlTable>)((tt) => tt.Add("xujsj", (long)0)) };
                yield return new object[] { "Remove", (Action<TomlTable>)((tt) => tt.Remove(MultiLevelTableScenario.RootXKey)) };
                yield return new object[] { "RemoveKvp", (Action<TomlTable>)((tt) => tt.Remove(new KeyValuePair<string, TomlObject>(MultiLevelTableScenario.RootXKey, null))) };
                yield return new object[] { "Clear", (Action<TomlTable>)((tt) => tt.Clear()) };
            }
        }

        [Theory(DisplayName = "When table was frozen write operation throws InvalidOp")]
        [MemberData(nameof(OperationsThatFailWhenFrozen))]
        public void WriteOperation_WhenTableIsFrozen_Returns(string _, Action<TomlTable> action)
        {
            using (var scenario = MultiLevelTableScenario.SetupFrozen())
            {

                // Act
                Action a = () => action(scenario.Table);

                // Assert
                a.ShouldThrow<InvalidOperationException>().WithMessage("*frozen*");
            }
        } 
    }
}
