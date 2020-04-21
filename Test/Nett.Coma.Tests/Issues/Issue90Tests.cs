using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests.Issues
{
    public class Issue90Tests
    {
        public interface ISomeInterface
        {
            int SomeInteger { get; set; }
            string SomeString { get; set; }
        }

        public class ClassA : ISomeInterface
        {
            public int SomeInteger { get; set; } = 1;
            public string SomeString { get; set; } = "This is Class A.";
            public string ClassAProperty { get; set; } = "Only for Class A!";
        }

        public class ClassB : ISomeInterface
        {
            public int SomeInteger { get; set; } = 2;
            public string SomeString { get; set; } = "This is Class B.";
            public string ClassBProperty { get; set; } = "Only for Class B!";
        }

        public class TomlEntity
        {
            public IDictionary<string, ISomeInterface> Dictionary { get; set; } =
                new Dictionary<string, ISomeInterface>();
        }


        class SD : Dictionary<string, string>
        {

        }

        [Fact]
        public void foox()
        {
            var i = new SD();
            var r = typeof(IDictionary).IsAssignableFrom(i.GetType());


            // Arrange
            var entity = new TomlEntity();
            var classA = new ClassA();
            var classB = new ClassB();
            entity.Dictionary.Add(nameof(ClassA), classA);
            entity.Dictionary.Add(nameof(ClassB), classB);
            var result = Toml.WriteString(entity);

            // act
            var settings = TomlSettings.Create(cfg => cfg
                .ConfigureType<IDictionary<string, ISomeInterface>>(ct => ct
                    .CreateInstance(() => new Dictionary<string, ISomeInterface>()))
                .ConfigureType<ISomeInterface>(tc => tc
                    .CreateInstance(ctx => ctx.Key switch
                        {
                            nameof(ClassA) => new ClassA(),
                            nameof(ClassB) => new ClassB(),
                            _ => throw new System.Exception($"Key '{ctx.Key}' not recognized as a 'ISomeInterface'."),
                        })));
            var read = Toml.ReadString<TomlEntity>(result, settings);

            // Assert
            read.Dictionary[nameof(ClassA)].Should().BeOfType<ClassA>();
            read.Dictionary[nameof(ClassB)].Should().BeOfType<ClassB>();

        }
    }
}
