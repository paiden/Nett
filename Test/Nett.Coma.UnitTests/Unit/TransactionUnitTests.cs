using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Nett.UnitTests.Util;
using NSubstitute;

namespace Nett.Coma.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class TransactionUnitTests
    {
        private readonly IMergeableConfig persistable;

        public TransactionUnitTests()
        {
            this.persistable = Substitute.For<IMergeableConfig>();
        }

        [MFact(nameof(Transaction), nameof(Transaction.Start), "when peristable is null throws arg null")]
        public void Constructor_WhenPersitableIsNull_ThrowsArgNull()
        {
            // Act
            Action a = () => Transaction.Start(null, _ => { });

            // Assert
            a.ShouldThrow<ArgumentNullException>().WithMessage("*persistable*");
        }

        [MFact(nameof(Transaction), nameof(Transaction.Start), "when callback is null throws arg null")]
        public void Constructor_WhenCallbackIsNull_ThrowsArgNull()
        {
            // Act
            Action a = () => Transaction.Start(this.persistable, null);

            // Assert
            a.ShouldThrow<ArgumentNullException>().WithMessage("*onCloseTransactionCallback*");
        }

        [MFact(nameof(Transaction), nameof(Transaction.Dispose), "invokes 'OnCloseCallback'")]
        public void Dispose_InvokesCallback()
        {
            // Arrange
            ManualResetEvent mre = new ManualResetEvent(false);

            // Act
            using (Transaction.Start(this.persistable, _ => mre.Set()))
            {
            }

            // Assert
            mre.WaitOne(TimeSpan.FromMilliseconds(100)).Should().Be(true);
        }

        [MFact(nameof(Transaction), nameof(Transaction.Save), "invokes persistable's save method")]
        public void Save_InvokesPersistableSave()
        {
            // Act
            using (Transaction.Start(this.persistable, _ => { }))
            {
            }

            // Assert
            this.persistable.ReceivedWithAnyArgs().Save(null);
        }

        [MFact(nameof(Transaction), nameof(Transaction.Save), "when persistable was changed externally throws InvalidOperationException")]
        public void Save_WhenPersistableWasChangedExternally_ThrowsInvalidOperation()
        {
            // Arrange
            this.persistable.WasChangedExternally().Returns(true);
            var t = Transaction.Start(this.persistable, _ => { });

            // Act
            Action a = () => t.Dispose();

            // Assert
            a.ShouldThrow<InvalidOperationException>("*externally*");
        }
    }
}
