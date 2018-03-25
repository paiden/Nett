using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;
using static System.FormattableString;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class Configuration
    {
        public bool EnableDebug { get; set; }
        public Server Server { get; set; }
        public Client Client { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ConfigurationWithDepdendency : Configuration
    {
        public ConfigurationWithDepdendency(object dependency)
        {

        }
    }

    [ExcludeFromCodeCoverage]
    public class Server
    {
        public TimeSpan Timeout { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Client
    {
        public string ServerAddress { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public struct Money
    {
        public string Currency { get; set; }
        public decimal Ammount { get; set; }

        public static Money Parse(string s) => new Money() { Ammount = decimal.Parse(s.Split(' ')[0]), Currency = s.Split(' ')[1] };
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
        private string exp = @"EnableDebug = true

[Server]
Timeout = 1m


[Client]
ServerAddress = ""http://127.0.0.1:8080""

";

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

            i.Should().Be(0);
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
    }
}