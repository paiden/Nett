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
                return this.Equals(obj as SystemConfig);
            }

            public bool Equals(SystemConfig config)
            {
                if(config == null) { return false; }

                return 
                    object.ReferenceEquals(this, config) ||
                    object.Equals(this.Env, config.Env) &&
                    this.Resources.SequenceEqual(config.Resources) &&
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

            public override bool Equals(object obj)
            {
                return this.Equals(obj as Environment);
            }

            public bool Equals(Environment env)
            {
                if(env == null) { return false; }
                return
                    object.ReferenceEquals(this, env) ||
                    object.Equals(this.Home, env.Home) &&
                    this.ProbeDirectories.SequenceEqual(env.ProbeDirectories) &&
                    this.Values.SequenceEqual(env.Values);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        private class Resource
        {
            public string Location { get; set; }

            public int Type { get; set; }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as Resource);
            }

            public bool Equals(Resource resource)
            {
                if(resource == null) { return false; }

                return
                    object.ReferenceEquals(this, resource) ||
                    object.Equals(this.Location, resource.Location) &&
                    object.Equals(this.Type, resource.Type);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
