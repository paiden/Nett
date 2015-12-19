using System;
using System.Collections.Generic;

namespace Nett.UnitTests
{
    public class PerformanceTests
    {
        public class TheTable
        {
            public int IntegerValue { get; set; }

            public float FloatValue { get; set; }

            public string SimpleString { get; set; }

            public string MultilineString { get; set; }

            public DateTime DateTime { get; set; }

            public TimeSpan TimeSpan { get; set; }

            public List<TheTable> SubTables { get; set; }
        }
    }
}
