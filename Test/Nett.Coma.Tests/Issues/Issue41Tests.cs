using System.IO;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests.Issues
{
    public sealed class Issue41Tests
    {
        public class Product
        {
            public string name { get; set; }
            public int number { get; set; }
        }

        public class Info
        {
            public Product[] product { get; set; }
        }

        public class ProductInfo
        {
            public Info info { get; set; }
        }

        [Fact(DisplayName = "Verify issue #41 was fixed: Loading config that uses TableArrays loads it correctly.")]
        public void LoadConfig_WhenConfigContainsTableArrays_LoadsThatConfigCorrectly_()
        {
            using (var fn = TestFileName.Create("productinfo", ".toml"))
            {
                File.WriteAllText(fn, @"
[info]
[[info.product]]
name = ""product1""
number = 10000
[[info.product]]
name = ""product2""
number = 10001
");

                var config = Config.CreateAs()
                    .MappedToType(() => new ProductInfo())
                    .StoredAs(store => store.File(fn))
                    .Initialize()
                    .Get(_ => _);

                config.info.product.Length.Should().Be(2);
                config.info.product[0].name.Should().Be("product1");
                config.info.product[1].name.Should().Be("product2");
            }

        }
    }
}
