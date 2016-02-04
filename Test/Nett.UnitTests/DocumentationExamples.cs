using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{

    public class Configuration
    {
        public bool EnableDebug { get; set; }
        public Server Server { get; set; }
        public Client Client { get; set; }
    }

    public class ConfigurationWithDepdendency : Configuration
    {
        public ConfigurationWithDepdendency(object dependency)
        {

        }
    }

    public class Server
    {
        public TimeSpan Timeout { get; set; }
    }

    public class Client
    {
        public string ServerAddress { get; set; }
    }

    public class TypeNotSupportedByToml
    {
        public Guid SomeGuid { get; set; }
    }

    public class DocumentationExamples
    {
        private string exp = @"EnableDebug = true

[Server]
Timeout = 00:01:00


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

        //[Fact]
        public void ReadTomlFileTest()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            var config = Toml.ReadFile<Configuration>(fn);

            config.EnableDebug.Should().Be(true);
            config.Client.ServerAddress.Should().Be("http://127.0.0.1:8080");
            config.Server.Timeout.Should().Be(TimeSpan.FromMinutes(1));
        }

        //[Fact]
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

        //[Fact]
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

        //[Fact]
        public void ReadNoDefaultConstructor_WhenActivatorRegistered_ThrowsInvalidOp()
        {
            string fn = this.NewFileName();
            this.WriteTomlFile(fn);

            //In Documentation
            var myConfig = TomlConfig.Create()
                .ConfigureType<ConfigurationWithDepdendency>()
                    .As.CreateWith(() => new ConfigurationWithDepdendency(new object()))
                .Apply();

            var config = Toml.ReadFile<ConfigurationWithDepdendency>(fn, myConfig);

            // Not in documentation

            config.EnableDebug.Should().Be(true);
            config.Client.ServerAddress.Should().Be("http://127.0.0.1:8080");
            config.Server.Timeout.Should().Be(TimeSpan.FromMinutes(1));
        }

        //[Fact]
        public void WriteGuidToml()
        {
            var obj = new TypeNotSupportedByToml() { SomeGuid = new Guid("6836AA79-AC1C-4173-8C58-0DE1791C8606") };

            var myconfig = TomlConfig.Create();
            //.ConfigureType<Guid>()
            //    .As.ConvertTo<TomlString>().As((g) => new TomlString(g.ToString()))
            //    .And.ConvertFrom<TomlString>().As((s) => new Guid(s.Value))
            //.Apply();

            Toml.WriteFile(obj, "test.tml", myconfig);
            var read = Toml.ReadFile<TypeNotSupportedByToml>("test.tml", myconfig);
        }
    }
}