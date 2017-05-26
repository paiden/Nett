using System;

namespace Nett.PerfTests
{
    internal class TomlV1
    {
        public int i { get; set; }
        public float f { get; set; }
        public string s1 { get; set; }
        public string s2 { get; set; }
        public string s3 { get; set; }
        public DateTime dt { get; set; }
        public TimeSpan ts { get; set; }
        public int[] ai { get; set; }
        public double[] af { get; set; }
        public string[] sa { get; set; }

        public TomlV1 Subtable { get; set; }
        public Product[] products { get; set; }
        public Fruit[] fruit { get; set; }
    }

    public class Product
    {
        public string name { get; set; }
        public int sku { get; set; }
        public string color { get; set; }
    }

    public class Fruit
    {
        public string name { get; set; }

        public Physical physical { get; set; }
        public Variety[] variety { get; set; }
    }

    public class Physical
    {
        public string color { get; set; }
        public string shape { get; set; }
    }

    public class Variety
    {
        public string name { get; set; }
    }
}
