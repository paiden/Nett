using System.Collections.Generic;
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
                case AlignmentMode.Global: return GetGlobalTableColumn((TomlTable)table.Root);
                default: return 0;
            }

            int GetBlockColumn()
            {
                return table.Rows.Max(r => r.Value.TomlType ==
                    TomlObjectType.Table || r.Value.TomlType == TomlObjectType.ArrayOfTables
                    ? 0 : r.Key.Length);
            }

            int GetGlobalTableColumn(TomlTable t) => GetGlobalRowsColumn(t.InternalRows);

            int GetGlobalTableArrayColumn(TomlTableArray tableArray) => tableArray.Items.Select(i => GetGlobalTableColumn(i)).Max();

            int GetGlobalRowsColumn(Dictionary<TomlKey, TomlObject> rows) => rows.Select(r => GetGlobalRowColumn(r)).Max();

            int GetGlobalRowColumn(KeyValuePair<TomlKey, TomlObject> row)
            {
                switch (row.Value)
                {
                    case TomlTable t: return GetGlobalTableColumn(t);
                    case TomlTableArray ta: return GetGlobalTableArrayColumn(ta);
                    default: return row.Key.Value.Length;
                }
            }
        }
    }
}
