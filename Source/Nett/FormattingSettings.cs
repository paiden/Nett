using System.Linq;

namespace Nett
{
    public enum AlignmentMode
    {
        None,
        Block,
        Global
    }

    internal sealed class FormattingSettings
    {
        public AlignmentMode AlignmentMode { private get; set; }

        internal int GetKeyValueAlignColumn(TomlTable table)
        {
            switch (this.AlignmentMode)
            {
                case AlignmentMode.Block: return GetBlockColumn();
                case AlignmentMode.Global: return GetGlobalColumn((TomlTable)table.Root);
                default: return 0;
            }

            int GetBlockColumn()
            {
                return table.Rows.Max(r => r.Value.TomlType == TomlObjectType.Table ? 0 : r.Key.Length);
            }

            int GetGlobalColumn(TomlTable current)
            {
                return current.Rows.Max(r => r.Value.TomlType == TomlObjectType.Table
                    ? GetGlobalColumn((TomlTable)r.Value)
                    : r.Key.Length);
            }
        }
    }
}
