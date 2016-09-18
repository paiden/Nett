using System;
using System.Linq.Expressions;
using Nett.Coma.Extensions;
using Xunit;

namespace Nett.Coma.Tests.Unit
{
    public sealed class LambdaExpressionExtensionTests
    {
        [Fact]
        public void Foo()
        {
            // Arrange
            Expression<Func<TestObject, L1Object>> x = (TestObject o) => o.L1;

            // Act
            var path = x.BuildTPath();

            // Assert
            path.Equals(TPath.Parse($"/{nameof(TestObject.L1)}"));
        }

        [Fact]
        public void FooA()
        {
            // Arrange
            const int Index = 1;
            Expression<Func<TestObject, L1Object>> x = (TestObject o) => o.L1Array[Index];

            // Act
            var path = x.BuildTPath();

            // Assert
            path.Equals(TPath.Parse($"/{nameof(TestObject.L1)}[{Index}]"));
        }


        [Fact]
        public void FooB()
        {
            // Arrange
            const int Index = 1;
            const int IndexA = 3;
            Expression<Func<TestObject, L1Object>> x = (TestObject o) => o.L1MArray[Index][IndexA];

            // Act
            var path = x.BuildTPath();

            // Assert
            path.Equals(TPath.Parse($"/{nameof(TestObject.L1)}[{Index}][{IndexA}]"));
        }


        private class TestObject
        {
            public L1Object L1 { get; set; }

            public L1Object[] L1Array { get; set; }

            public L1Object[][] L1MArray { get; set; }
        }

        private class L1Object
        {

        }
    }
}
