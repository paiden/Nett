using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class ComaConfigTests
    {
        [Fact(DisplayName = "Load with null as file locations throws argument null exception.")]
        public void LoadMergedConfig_WhenLocationsIsNull_ThrowsArgNull()
        {
            // Act
            Action a = () => Config.CreateAs()
                .MappedToType(() => new SingleLevelConfig())
                .StoredAs(store => store.File((string)null))
                .Initialize();

            // Assert
            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact(DisplayName = "Load with null as default creator throws argument null exception.")]
        public void LoadMergedConfig_WhenDefaultCreatorIsNull_ThrowsArgNull()
        {
            // Act
            Action a = () => Config.CreateAs()
                .MappedToType<SingleLevelConfig>(null)
                .StoredAs(store => store.File("x"));

            // Assert
            a.ShouldThrow<ArgumentNullException>();
        }
    }
}
