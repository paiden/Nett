using System;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests.Internal.Unit
{
    public sealed class StoreBuilderTests
    {
        [Fact]
        public void CustomStore_WhenSoreIsNull_ThrowsArgNull()
        {
            // Arrange
            var sb = new Config.StoreBuilder() as Config.IStoreBuilder;

            // Act
            Action a = () => sb.CustomStore(null);


            // Assert
            a.ShouldThrow<ArgumentNullException>().WithMessage("*store*");
        }
    }
}
