using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    /// <summary>
    /// Read/Write one larger TOML file with many different components.
    /// </summary>
    public class MasterTests
    {
        [Fact]
        public void SerializeAndDeseralize_WorksCorrectly()
        {
            // Arrange
            var toSerialize = this.CreateTestConfig();

            // Act
            var written = Toml.WriteString(toSerialize);
            var read = Toml.Read<SystemConfig>(written);

            // Assert
            Assert.Equal(toSerialize, read);
        }


        private SystemConfig CreateTestConfig()
        {
            return new SystemConfig()
            {
                Env = new Environment()
                {
                    Home = "C:\\test.txt",
                    ProbeDirectories = new string[]
                    {
                        "C:\\dir1",
                        "C:\\dir2",
                        "C:\\dir3",
                    },
                    Values = new List<double>(),
                },
                Resources = new List<Resource>()
                {
                    new Resource()
                    {
                        Location = "Some weird location \r\n that contains espace sequences",
                        Type = -100,
                    },
                    new Resource()
                    {
                        Location = "C:\\somelocation",
                        Type = 229
                    }
                },
            };
        }

        private class SystemConfig
        {
            public Environment Env { get; set; }
            public List<Resource> Resources { get; set; }

            public Environment Env2 { get; set; }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public bool Euqals(SystemConfig config)
            {
                if(config == null) { return false; }

                return object.Equals(this.Env, config.Env) &&
                    object.Equals(this.Resources, config.Resources) &&
                    object.Equals(this.Env2, config.Env2);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class Environment
        {
            public string Home { get; set; }
            public string[] ProbeDirectories { get; set; }
            public List<double> Values { get; set; }
        }

        private class Resource
        {
            public string Location { get; set; }

            public int Type { get; set; }
        }
    }
}
