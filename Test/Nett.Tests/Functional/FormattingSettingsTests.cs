using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using static Nett.TomlSettings;

namespace Nett.Tests.Functional
{
    public sealed class FormattingSettingsTests
    {
        public class Settings
        {
            [TomlComment("Group")]
            public string Group { get; set; } = "A";

            [TomlComment("This is a root setting")]
            public string SytemName { get; set; } = "TestSystem";

            [TomlComment("Unique system Id")]
            public int Id { get; set; } = 3939;

            [TomlComment("Short system alias")]
            public string Alias { get; set; } = "Test";

            public int MaxClients { get; set; } = 123;

            public Server MA { get; set; } = new Server();
            public Server MB { get; set; } = new Server();
            public Client CA { get; set; } = new Client();
            public Client CB { get; set; } = new Client();

            public class Server
            {
                public string IP { get; set; } = "127.0.0.1";

                public Connection Primary { get; set; } = new Connection() { Address = "http://primary.com" };
                public Connection Fallback { get; set; } = new Connection() { Address = "file://netsharefallbackfile.txt" };

                public List<Connection> MostRecent { get; set; } = new List<Connection>()
                {
                    new Connection() { Address = "http://mr1.com", Port = 4 },
                    new Connection() { Address = "htt://mostrecent2.com/subpage/crash", Port = 5 },
                };

                public class Connection
                {
                    [TomlComment("SC.")]
                    public string Address { get; set; } = "http://test.com";
                    public int Port { get; set; } = 1234;
                }
            }

            public class Client
            {
                public string Name { get; set; } = "ClientX";
                [TomlComment("This is a super comment.")]
                [TomlComment("What does a second comment do?")]
                public int Priority { get; set; } = 100;
                public ErrorHandling ErrorBehaviorr = new ErrorHandling();

                public class ErrorHandling
                {
                    public string Policy { get; set; } = "CrashOnError";
                    public int RetryCount { get; set; } = -1;
                }
            }
        }

        [Fact]
        public void WriteToml_WhenDefaultFormattingIsUsed_NiceTomlGetsWrittenHeHe()
        {
            // Arrange
            var settings = TomlSettings.Create(cfg => cfg
                .Apply(CommonConfig));

            // Act
            var s = Toml.WriteString(new Settings(), settings);

            // Assert
            const string expected = @"#Group
Group      = 'A'
#This is a root setting
SytemName  = 'TestSystem'
#Unique system Id
Id         = 3939
#Short system alias
Alias      = 'Test'
MaxClients = 123

[MA]
IP = '127.0.0.1'

    [MA.Primary]
    #SC.
    Address = 'http://primary.com'
    Port    = 1234

    [MA.Fallback]
    #SC.
    Address = 'file://netsharefallbackfile.txt'
    Port    = 1234

    [[MA.MostRecent]]
    #SC.
    Address = 'http://mr1.com'
    Port    = 4
    [[MA.MostRecent]]
    #SC.
    Address = 'htt://mostrecent2.com/subpage/crash'
    Port    = 5

[MB]
IP = '127.0.0.1'

    [MB.Primary]
    #SC.
    Address = 'http://primary.com'
    Port    = 1234

    [MB.Fallback]
    #SC.
    Address = 'file://netsharefallbackfile.txt'
    Port    = 1234

    [[MB.MostRecent]]
    #SC.
    Address = 'http://mr1.com'
    Port    = 4
    [[MB.MostRecent]]
    #SC.
    Address = 'htt://mostrecent2.com/subpage/crash'
    Port    = 5

[CA]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100

[CB]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100
";
            s.Should().Be(expected);
        }

        [Fact]
        public void WriteToml_WhenBlockAlignIsUsed_TomlIsWrittenCorrectly()
        {
            // Arrange
            var settings = TomlSettings.Create(cfg => cfg
                .Apply(CommonConfig)
                .ConfigureFormatting(fmt =>
                    fmt.UseKeyValueAlignment(AlignmentMode.Block)));

            // Act
            var s = Toml.WriteString(new Settings(), settings);

            // Assert
            const string expected = @"#Group
Group      = 'A'
#This is a root setting
SytemName  = 'TestSystem'
#Unique system Id
Id         = 3939
#Short system alias
Alias      = 'Test'
MaxClients = 123

[MA]
IP = '127.0.0.1'

    [MA.Primary]
    #SC.
    Address = 'http://primary.com'
    Port    = 1234

    [MA.Fallback]
    #SC.
    Address = 'file://netsharefallbackfile.txt'
    Port    = 1234

    [[MA.MostRecent]]
    #SC.
    Address = 'http://mr1.com'
    Port    = 4
    [[MA.MostRecent]]
    #SC.
    Address = 'htt://mostrecent2.com/subpage/crash'
    Port    = 5

[MB]
IP = '127.0.0.1'

    [MB.Primary]
    #SC.
    Address = 'http://primary.com'
    Port    = 1234

    [MB.Fallback]
    #SC.
    Address = 'file://netsharefallbackfile.txt'
    Port    = 1234

    [[MB.MostRecent]]
    #SC.
    Address = 'http://mr1.com'
    Port    = 4
    [[MB.MostRecent]]
    #SC.
    Address = 'htt://mostrecent2.com/subpage/crash'
    Port    = 5

[CA]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100

[CB]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100
";
            s.Should().Be(expected);
        }

        [Fact]
        public void WriteToml_WhenGlobalAlignIsUsed_TomlIsWrittenCorrectly()
        {
            // Arrange
            var settings = TomlSettings.Create(cfg => cfg
                .Apply(CommonConfig)
                .ConfigureFormatting(fmt =>
                    fmt.UseKeyValueAlignment(AlignmentMode.Global)));

            // Act
            var s = Toml.WriteString(new Settings(), settings);

            // Assert
            const string expected = @"#Group
Group      = 'A'
#This is a root setting
SytemName  = 'TestSystem'
#Unique system Id
Id         = 3939
#Short system alias
Alias      = 'Test'
MaxClients = 123

[MA]
IP         = '127.0.0.1'

    [MA.Primary]
    #SC.
    Address    = 'http://primary.com'
    Port       = 1234

    [MA.Fallback]
    #SC.
    Address    = 'file://netsharefallbackfile.txt'
    Port       = 1234

    [[MA.MostRecent]]
    #SC.
    Address    = 'http://mr1.com'
    Port       = 4
    [[MA.MostRecent]]
    #SC.
    Address    = 'htt://mostrecent2.com/subpage/crash'
    Port       = 5

[MB]
IP         = '127.0.0.1'

    [MB.Primary]
    #SC.
    Address    = 'http://primary.com'
    Port       = 1234

    [MB.Fallback]
    #SC.
    Address    = 'file://netsharefallbackfile.txt'
    Port       = 1234

    [[MB.MostRecent]]
    #SC.
    Address    = 'http://mr1.com'
    Port       = 4
    [[MB.MostRecent]]
    #SC.
    Address    = 'htt://mostrecent2.com/subpage/crash'
    Port       = 5

[CA]
Name       = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority   = 100

[CB]
Name       = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority   = 100
";
            s.Should().Be(expected);
        }

        [Fact]
        public void WriteToml_UsingTableIndentationOf4_SubTalesGetIndentedCorrectly()
        {
            // Arrange
            var settings = TomlSettings.Create(cfg => cfg
                .Apply(CommonConfig)
                .ConfigureFormatting(fmt => fmt
                    .IndentTablesBy(4)));

            // Act
            var s = Toml.WriteString(new Settings(), settings);

            // Assert
            const string expected = @"#Group
Group      = 'A'
#This is a root setting
SytemName  = 'TestSystem'
#Unique system Id
Id         = 3939
#Short system alias
Alias      = 'Test'
MaxClients = 123

[MA]
IP = '127.0.0.1'

    [MA.Primary]
    #SC.
    Address = 'http://primary.com'
    Port    = 1234

    [MA.Fallback]
    #SC.
    Address = 'file://netsharefallbackfile.txt'
    Port    = 1234

    [[MA.MostRecent]]
    #SC.
    Address = 'http://mr1.com'
    Port    = 4
    [[MA.MostRecent]]
    #SC.
    Address = 'htt://mostrecent2.com/subpage/crash'
    Port    = 5

[MB]
IP = '127.0.0.1'

    [MB.Primary]
    #SC.
    Address = 'http://primary.com'
    Port    = 1234

    [MB.Fallback]
    #SC.
    Address = 'file://netsharefallbackfile.txt'
    Port    = 1234

    [[MB.MostRecent]]
    #SC.
    Address = 'http://mr1.com'
    Port    = 4
    [[MB.MostRecent]]
    #SC.
    Address = 'htt://mostrecent2.com/subpage/crash'
    Port    = 5

[CA]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100

[CB]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100
";
            s.Should().Be(expected);
        }

        [Fact]
        public void WriteToml_UsingTableIndentationOf2_SubTalesGetIndentedCorrectly()
        {
            // Arrange
            var settings = TomlSettings.Create(cfg => cfg
                .Apply(CommonConfig)
                .ConfigureFormatting(fmt => fmt
                    .IndentTablesBy(2)));

            // Act
            var s = Toml.WriteString(new Settings(), settings);

            // Assert
            const string expected = @"#Group
Group      = 'A'
#This is a root setting
SytemName  = 'TestSystem'
#Unique system Id
Id         = 3939
#Short system alias
Alias      = 'Test'
MaxClients = 123

[MA]
IP = '127.0.0.1'

  [MA.Primary]
  #SC.
  Address = 'http://primary.com'
  Port    = 1234

  [MA.Fallback]
  #SC.
  Address = 'file://netsharefallbackfile.txt'
  Port    = 1234

  [[MA.MostRecent]]
  #SC.
  Address = 'http://mr1.com'
  Port    = 4
  [[MA.MostRecent]]
  #SC.
  Address = 'htt://mostrecent2.com/subpage/crash'
  Port    = 5

[MB]
IP = '127.0.0.1'

  [MB.Primary]
  #SC.
  Address = 'http://primary.com'
  Port    = 1234

  [MB.Fallback]
  #SC.
  Address = 'file://netsharefallbackfile.txt'
  Port    = 1234

  [[MB.MostRecent]]
  #SC.
  Address = 'http://mr1.com'
  Port    = 4
  [[MB.MostRecent]]
  #SC.
  Address = 'htt://mostrecent2.com/subpage/crash'
  Port    = 5

[CA]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100

[CB]
Name     = 'ClientX'
#This is a super comment.
#What does a second comment do?
Priority = 100
";
            s.Should().Be(expected);
        }

        private static void CommonConfig(ITomlSettingsBuilder builder)
        {
            builder.UseDefaultStringType(TomlStringType.Literal);
        }
    }
}
