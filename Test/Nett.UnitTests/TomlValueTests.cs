using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    public class TomlValueTests
    {
        [Fact]
        public void Get_WhenTargetTypeIsItsOwnType_CanAlwaysConvert()
        {
            var val = new TomlString("SomeString");

            var conv = val.Get<TomlString>();

            conv.Should().NotBeNull();
            conv.Value.Should().Be("SomeString");
        }
    }
}
