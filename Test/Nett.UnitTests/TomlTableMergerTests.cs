using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace Nett.UnitTests
{
    public class TomlTableMergerTests
    {
        public void Merge_WhenTargetTableDoesntCotainRow_MergerTakesFromRow()
        {
            var from = this.CreateSingleProp();
            var to = this.CreateEmpty();

            var r = TomlTableMerger.ValuesOverwrittenFromTo(from, to);

            r.Rows.Count().Should().Be(1);
            r.Get("r1").As<TomlInt>().Value.Should().Be(1);
        }

        private TomlTable CreateEmpty()
        {
            return new TomlTable();
        }

        private TomlTable CreateSingleProp()
        {
            var tt = new TomlTable();
            tt.Add("r1", new TomlInt(1));
            return tt;
        }
    }
}
