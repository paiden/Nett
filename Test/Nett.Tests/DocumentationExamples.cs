using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;
using Xunit.Abstractions;
using static System.FormattableString;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class Configuration
    {
        public bool EnableDebug { get; set; }
        public Server Server { get; set; } = new Server();
        public Client Client { get; set; } = new Client();
    }

    [ExcludeFromCodeCoverage]
    public class Server
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
    }

    [ExcludeFromCodeCoverage]
    public class Client
    {
        public string ServerAddress { get; set; } = "http://localhost:8082";
    }


    [ExcludeFromCodeCoverage]
    public class ConfigurationWithDepdendency : Configuration
    {
        public ConfigurationWithDepdendency(object dependency)
        {

        }
    }

    [ExcludeFromCodeCoverage]
    public struct Money
    {
        public string Currency { get; set; }
        public decimal Ammount { get; set; }

        public static Money Parse(string s) => new Money() { Ammount = decimal.Parse(s.Split(' ')[0], CultureInfo.InvariantCulture), Currency = s.Split(' ')[1] };
        public override string ToString() => Invariant($"{this.Ammount} {this.Currency}");
    }

    [ExcludeFromCodeCoverage]
    public class TableContainingMoney
    {
        public Money NotSupported { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class TypeNotSupportedByToml
    {
        public Guid SomeGuid { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public sealed class Computed
    {
        public int X { get; set; } = 1;
        public int Y { get; set; } = 2;

        //[TomlIgnore]
        public int Z => X + Y;
    }

    [ExcludeFromCodeCoverage]
    public class DocumentationExamples
    {


        private string exp = @"
EnableDebug = true

[Server]
Timeout = 1m

[Client]
ServerAddress = ""http://127.0.0.1:8080""
";

        private readonly ITestOutputHelper Console;

        public DocumentationExamples(ITestOutputHelper console)
        {
            this.Console = console;
        }

        private string NewFileName() => Guid.NewGuid() + ".toml";

        private void WriteTomlFile(string fileName)
        {
            var config = new Configuration()
            {
                EnableDebug = true,
                Server = new Server() { Timeout = TimeSpan.FromMinutes(1) },
                Client = new Client() { ServerAddress = "http://127.0.0.1:8080" },
            };

            Toml.WriteFile(config, fileName);
        }

        [Fact]
        public void WriteTomlFileTest()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            // Not in documentation
            using (var s = File.Open(fn, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(s))
            {
                var sc = sr.ReadToEnd();
                sc.ShouldBeSemanticallyEquivalentTo(exp);
            }
        }

        [Fact]
        public void ReadTomlFileTest()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            var config = Toml.ReadFile<Configuration>(fn);

            config.EnableDebug.Should().Be(true);
            config.Client.ServerAddress.Should().Be("http://127.0.0.1:8080");
            config.Server.Timeout.Should().Be(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void ReadFileUntyped()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            // In Documentation
            TomlTable table = Toml.ReadFile(fn);
            var timeout = table.Get<TomlTable>("Server").Get<TimeSpan>("Timeout");

            // Not in documentation
            timeout.Should().Be(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void ReadNoDefaultConstructor_WhenNoActivatorRegistered_ThrowsInvalidOp()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            //In Documentation
            Action a = () =>
            {
                var config = Toml.ReadFile<ConfigurationWithDepdendency>(fn);
            };

            // Not in documentation
            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ReadNoDefaultConstructor_WhenActivatorRegistered_ThrowsInvalidOp()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            //In Documentation
            var myConfig = TomlSettings.Create(cfg => cfg
                .ConfigureType<ConfigurationWithDepdendency>(ct => ct
                    .CreateInstance(() => new ConfigurationWithDepdendency(new object()))));

            var config = Toml.ReadFile<ConfigurationWithDepdendency>(fn, myConfig);

            // Not in documentation

            config.EnableDebug.Should().Be(true);
            config.Client.ServerAddress.Should().Be("http://127.0.0.1:8080");
            config.Server.Timeout.Should().Be(TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void HandleComputedType()
        {
            var c = new Computed();
            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<Computed>(type => type
                    .IgnoreProperty(o => o.Z)));

            var w = Toml.WriteString(c, config);
            var r = Toml.ReadString<Computed>(w, config);

        }

        [Fact]
        public void WriteGuidToml()
        {
            var obj = new TableContainingMoney()
            {
                NotSupported = new Money() { Ammount = 9.99m, Currency = "EUR" }
            };

            //var config = TomlConfig.Create(cfg => cfg
            //    .ConfigureType<decimal>(type => type
            //        .WithConversionFor<TomlFloat>(convert => convert
            //            .ToToml(dec => (double)dec)
            //            .FromToml(tf => (decimal)tf.Value))));

            var config = TomlSettings.Create(cfg => cfg
                .ConfigureType<Money>(type => type
                    .WithConversionFor<TomlString>(convert => convert
                        .ToToml(custom => custom.ToString())
                        .FromToml(tmlString => Money.Parse(tmlString.Value)))));

            //var config = TomlConfig.Create();
            var s = Toml.WriteString(obj, config);
            var read = Toml.ReadString<TableContainingMoney>(s, config);
        }

        [Fact]
        public void ActivateAll()
        {
            var config = TomlSettings.Create(cfg => cfg.AllowImplicitConversions(TomlSettings.ConversionSets.All));
            var tbl = Toml.ReadString("f = 0.99", config);
            var i = tbl.Get<int>("f");

            i.Should().Be(1);
        }

        [Fact]
        public void ActivateNone()
        {
            var config = TomlSettings.Create(cfg => cfg.AllowImplicitConversions(TomlSettings.ConversionSets.None));
            var tbl = Toml.ReadString("i = 1", config);
            // var i = tbl.Get<int>("i"); // Would throw InvalidOperationException as event cast from TomlInt to int is not allowed
            var i = tbl.Get<long>("i"); // Only this will work

            i.Should().Be(1);
        }

        [Fact]
        public void Read_AsTomlTable_ReadsTableCorrectly()
        {
            // Arrange
            using (var filename = TestFileName.Create("test", ".toml"))
            {
                File.WriteAllText(filename, exp);

                // Act
                var toml = Toml.ReadFile(filename);
                Console.WriteLine("EnableDebug: " + toml.Get<bool>("EnableDebug"));
                Console.WriteLine("Timeout: " + toml.Get<TomlTable>("Server").Get<TimeSpan>("Timeout"));
                Console.WriteLine("ServerAddress: " + toml.Get<TomlTable>("Client").Get<string>("ServerAddress"));

                // Assert
                toml.Get<bool>("EnableDebug").Should().Be(true);
                toml.Get<TomlTable>("Server").Get<TimeSpan>("Timeout").Should().Be(TimeSpan.FromMinutes(1));
                toml.Get<TomlTable>("Client").Get<string>("ServerAddress").Should().Be("http://127.0.0.1:8080");
            }
        }

        [Fact]
        public void Read_AsDictionary_ReadsTableCorrectly()
        {
            // Arrange
            using (var filename = TestFileName.Create("test", ".toml"))
            {
                File.WriteAllText(filename, exp);

                // Act
                var data = Toml.ReadFile(filename).ToDictionary();
                var server = (Dictionary<string, object>)data["Server"];
                var client = (Dictionary<string, object>)data["Client"];

                Console.WriteLine("EnableDebug: " + data["EnableDebug"]);
                Console.WriteLine("Timeout: " + server["Timeout"]);
                Console.WriteLine("ServerAddress: " + client["ServerAddress"]);

                // Assert
                data["EnableDebug"].Should().Be(true);
                server["Timeout"].Should().Be(TimeSpan.FromMinutes(1));
                client["ServerAddress"].Should().Be("http://127.0.0.1:8080");
            }
        }

        [Fact]
        public void Read_AsCustomObject_ReadsTableCorrectly()
        {
            // Arrange
            using (var filename = TestFileName.Create("test", ".toml"))
            {
                File.WriteAllText(filename, exp);

                // Act
                var cust = Toml.ReadFile<Configuration>(filename);

                Console.WriteLine("EnableDebug: " + cust.EnableDebug);
                Console.WriteLine("Timeout: " + cust.Server.Timeout);
                Console.WriteLine("ServerAddress: " + cust.Client.ServerAddress);

                // Assert
                cust.EnableDebug.Should().Be(true);
                cust.Server.Timeout.Should().Be(TimeSpan.FromMinutes(1));
                cust.Client.ServerAddress.Should().Be("http://127.0.0.1:8080");
            }
        }

        [Fact]
        public void Write_WithCustomObject_WritesCorrectFileContent()
        {
            // Arrange
            using (var filename = TestFileName.Create("test", ".toml"))
            {
                // Act
                var obj = new Configuration();
                Toml.WriteFile(obj, filename);

                // Assert
                File.ReadAllText(filename).ShouldBeSemanticallyEquivalentTo(
                    @"
EnableDebug = false
[Server]
Timeout = 2m
[Client]
ServerAddress = ""http://localhost:8082""");
            }
        }

        [Fact]
        public void Write_WithTomlTable_WritesCorrectFileContent()
        {
            // Arrange
            using (var filename = TestFileName.Create("test", ".toml"))
            {
                // Act
                var server = Toml.Create();
                server.Add("Timeout", TimeSpan.FromMinutes(2));

                var client = Toml.Create();
                client.Add("ServerAddress", "http://localhost:8082");

                var tbl = Toml.Create();
                tbl.Add("EnableDebug", false);
                tbl.Add("Server", server);
                tbl.Add("Client", client);

                Toml.WriteFile(tbl, filename);

                // Assert
                File.ReadAllText(filename).ShouldBeSemanticallyEquivalentTo(
                    @"
EnableDebug = false
[Server]
Timeout = 2m
[Client]
ServerAddress = ""http://localhost:8082""");
            }
        }

        [Fact]
        public void Write_WithDictionary_WritesCorrectFileContent()
        {
            // Arrange
            using (var filename = TestFileName.Create("test", ".toml"))
            {
                // Act
                var data = new Dictionary<string, object>()
                {
                    { "EnableDebug", false },
                    { "Server", new Dictionary<string, object>() { { "Timeout", TimeSpan.FromMinutes(2) } } },
                    { "Client", new Dictionary<string, object>() { { "ServerAddress", "http://localhost:8082" } } },
                };

                Toml.WriteFile(data, filename);

                // Assert
                File.ReadAllText(filename).ShouldBeSemanticallyEquivalentTo(
                    @"
EnableDebug = false
[Server]
Timeout = 2m
[Client]
ServerAddress = ""http://localhost:8082""");
            }

        }
    }
}
