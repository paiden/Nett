using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    public class ToDictionaryTests
    {
        private const string Src = @"
# This is a TOML document.

title = ""TOML Example""

[owner]
        name = ""Tom Preston-Werner""
dob = 1979-05-27T07:32:00-08:00 # First class dates

[database]
        server = ""192.168.1.1""
ports = [ 8001, 8001, 8002 ]
        connection_max = 5000
enabled = true

[servers]

# Indentation (tabs and/or spaces) is allowed but not required
        [servers.alpha]
        ip = ""10.0.0.1""
  dc = ""eqdc10""

  [servers.beta]
        ip = ""10.0.0.2""
  dc = ""eqdc10""

[clients]
        data = [ [""gamma"", ""delta""], [1, 2] ]

# Line breaks are OK when inside arrays
hosts = [
  ""alpha"",
  ""omega""
]";

        [Fact(DisplayName = "To Dictionary should produce correct plain CLR objects result")]
        [Description("Only very simple test so that I can do faster debugging later, the real testing of this will be done by incorporating the GO TOML test suite.")]
        public void ToDictionary_ProducesCorrectResult()
        {
            // Arrange
            var tml = Toml.ReadString(Src);

            // Act
            var dict = tml.ToDictionary();

            // Assert
            dict["title"].Should().Be("TOML Example");
            dict["owner"].Should().BeOfType<Dictionary<string, object>>();

            var owner = (Dictionary<string, object>)dict["owner"];
            owner["name"].Should().Be("Tom Preston-Werner");
        }
    }
}
