namespace Nett.AspNet.SystemTests
{
    public class TestOptions
    {
        public TestOptions()
        {
            this.Option1 = "value1_from_ctor";
        }

        public string Option1 { get; set; }

        public int Option2 { get; set; } = 5;
    }

    public class MySubOptions
    {
        public MySubOptions()
        {
            // Set default values.
            this.SubOption1 = "value1_from_ctor";
            this.SubOption2 = 5;
        }

        public string SubOption1 { get; set; }

        public int SubOption2 { get; set; }
    }
}
