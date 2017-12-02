using System;
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
            public Server MA { get; set; } = new Server();
            public Server MB { get; set; } = new Server();
            public Client CA { get; set; } = new Client();
            public Client CB { get; set; } = new Client();

            public class Server
            {
                public string IP { get; set; } = "127.0.0.1";

                public Connection Primary { get; set; } = new Connection();
                public Connection Fallback { get; set; } = new Connection();

                public List<Connection> MostRecent { get; set; } = new List<Connection>()
                {
                    new Connection() { Port = 4 },
                    new Connection() { Port = 5 },
                };

                public class Connection
                {
                    [TomlComment("This is an appended comment.", CommentLocation.Append)]
                    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
                    public int Port { get; set; } = 1234;
                }
            }

            public class Client
            {
                public string Name { get; set; } = "ClientX";
                [TomlComment("This is a prepended comment.", CommentLocation.Prepend)]
                [TomlComment("What does a second comment do?", CommentLocation.Prepend)]
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
            const string expected = @"
[MA]
IP = '127.0.0.1'

[MA.Primary]
Timeout = 00:01:00 #This is an appended comment.
Port    = 1234

[MA.Fallback]
Timeout = 00:01:00 #This is an appended comment.
Port    = 1234

[[MA.MostRecent]]
Timeout = 00:01:00 #This is an appended comment.
Port    = 4
[[MA.MostRecent]]
Timeout = 00:01:00 #This is an appended comment.
Port    = 5

[MB]
IP = '127.0.0.1'

[MB.Primary]
Timeout = 00:01:00 #This is an appended comment.
Port    = 1234

[MB.Fallback]
Timeout = 00:01:00 #This is an appended comment.
Port    = 1234

[[MB.MostRecent]]
Timeout = 00:01:00 #This is an appended comment.
Port    = 4
[[MB.MostRecent]]
Timeout = 00:01:00 #This is an appended comment.
Port    = 5

[CA]
Name     = 'ClientX'
#This is a prepended comment.
#What does a second comment do?
Priority = 100

[CB]
Name     = 'ClientX'
#This is a prepended comment.
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
            const string expected = @"
[MA]
IP       = '127.0.0.1'

[MA.Primary]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 1234

[MA.Fallback]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 1234

[[MA.MostRecent]]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 4
[[MA.MostRecent]]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 5

[MB]
IP       = '127.0.0.1'

[MB.Primary]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 1234

[MB.Fallback]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 1234

[[MB.MostRecent]]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 4
[[MB.MostRecent]]
Timeout  = 00:01:00 #This is an appended comment.
Port     = 5

[CA]
Name     = 'ClientX'
#This is a prepended comment.
#What does a second comment do?
Priority = 100

[CB]
Name     = 'ClientX'
#This is a prepended comment.
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
