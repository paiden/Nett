using System;

namespace Nett.Coma.Test.Util
{
    public sealed class MultiLevelTableScenario : IDisposable
    {
        public const string RootXKey = "X";

        private MultiLevelTableScenario()
        {
            this.Table = Toml.Create(this.Clr);
        }

        public ClrObject Clr { get; private set; } = new ClrObject();
        public TomlTable Table { get; private set; }

        public TomlTable SubTable => ((TomlTable)this.Table["Sub"]);

        public static MultiLevelTableScenario Setup()
        {
            return new MultiLevelTableScenario();
        }

        public static MultiLevelTableScenario SetupFrozen()
        {
            var scenario = Setup();

            scenario.Table.Freeze();

            return scenario;
        }

        public void Dispose()
        {

        }

        public class ClrObject
        {
            public int X { get; set; } = 1;

            public ClrSubObject Sub { get; set; } = new ClrSubObject();
        }

        public class ClrSubObject
        {
            public string Y { get; set; } = "Y";
        }
    }
}
