using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Internal
{
    public class TomlValueTests
    {
        [Fact]
        public void Get_WhenTargetTypeIsItsOwnType_CanAlwaysConvert()
        {
            var val = new TomlString(new TomlTable.RootTable(TomlConfig.DefaultInstance), "SomeString");

            var conv = val.Get<TomlString>();

            conv.Should().NotBeNull();
            conv.Value.Should().Be("SomeString");
        }
    }
}
