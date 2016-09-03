namespace Nett.Coma.Tests.Functional
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using Nett.UnitTests.Util;
    using TestData;

    [ExcludeFromCodeCoverage]
    public sealed class TransactionTests
    {
        private const string FuncUseTransaction = "Use Transaction";

        [FFact(FuncUseTransaction, "will apply the changes to the destination file")]
        public void UseTransaction_WillApplySettings()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(UseTransaction_WillApplySettings)))
            {
                // Arrange
                const int ChangedX = 234;
                var cfg = scenario.CreateConfig();

                // Act
                using (cfg.StartTransaction())
                {
                    cfg.Set(c => c.X = 2);
                    cfg.Set(c => c.X = 23);
                    cfg.Set(c => c.X = ChangedX);
                }

                // Assert
                scenario.ReadFile().X.Should().Be(ChangedX);
            }
        }


        [FFact(FuncUseTransaction, "after transaction is closed, saving will directly save again")]
        public void UseTransaction_AfterTransactionIsClosed_SaveWillSaveDirectlyAgain()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(UseTransaction_AfterTransactionIsClosed_SaveWillSaveDirectlyAgain)))
            {
                // Arrange
                const int ChangedX = 234;
                var cfg = scenario.CreateConfig();
                using (cfg.StartTransaction())
                {
                    cfg.Set(c => c.X = 2);
                    cfg.Set(c => c.X = 23);
                }

                // Act
                cfg.Set(c => c.X = ChangedX);

                // Assert
                scenario.ReadFile().X.Should().Be(ChangedX);
            }
        }

        [FFact(FuncUseTransaction, "changes will not be applied if transaction is not disposed")]
        public void UseTransaction_ChangesNotSavedUntilTransactionIsDisposed()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(UseTransaction_ChangesNotSavedUntilTransactionIsDisposed)))
            {
                // Arrange
                const int ChangedX = 234;
                var cfg = scenario.CreateConfig();
                int original = cfg.Get(c => c.X);
                cfg.StartTransaction();

                // Act
                cfg.Set(c => c.X = 2);
                cfg.Set(c => c.X = 23);
                cfg.Set(c => c.X = ChangedX);

                // Assert
                scenario.ReadFile().X.Should().Be(original);
            }
        }

        [FFact(FuncUseTransaction, "only one transaction can be active at any time")]
        public void UseTransaction_OnlyOneTransactionAllowed()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(UseTransaction_OnlyOneTransactionAllowed)))
            {
                // Arrange
                var cfg = scenario.CreateConfig();
                cfg.StartTransaction();

                // Act
                Action a = () => cfg.StartTransaction();

                // Assert
                a.ShouldThrow<InvalidOperationException>().WithMessage("*transaction*");
            }
        }

        [FFact(FuncUseTransaction, "while transaction is active only in memory table is used and external changes are ignored")]
        public void UseTransaction_ExternalChangesAreLoaded()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(UseTransaction_ExternalChangesAreLoaded)))
            {
                // Arrange
                var cfg = scenario.CreateConfig();
                cfg.StartTransaction();
                int originalValue = cfg.Get(c => c.X);
                const int newValue = 123;
                Toml.WriteFile(new SingleConfigFileScenario.ConfigContent() { X = newValue }, scenario.File);

                // Act
                var v = cfg.Get(c => c.X);

                // Assert
                v.Should().Be(originalValue);
            }
        }

        [FFact(FuncUseTransaction, "external changes cause an concurrency exception when transaction is closed")]
        public void UseTransaction_ExternalChangesNotIncorporatedAndSavedBack()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(UseTransaction_ExternalChangesAreLoaded)))
            {
                // Arrange
                var cfg = scenario.CreateConfig();
                var trans = cfg.StartTransaction();
                const int newXValue = 123;
                const string newYValue = "New Y";
                Toml.WriteFile(new SingleConfigFileScenario.ConfigContent() { X = newXValue }, scenario.File);

                // Act
                cfg.Set(c => c.Y = newYValue);
                Action a = () => trans.Dispose();

                // Assert
                a.ShouldThrow<InvalidOperationException>().WithMessage("*externally*");
            }
        }
    }
}
