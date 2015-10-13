using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nett
{
    internal static class TomlTableMerger
    {
        internal static TomlTable ValuesOverwrittenFromTo(TomlTable mergeFrom, TomlTable mergeTo)
        {
            Debug.Assert(mergeFrom != null);
            Debug.Assert(mergeTo != null);

            TomlTable merged = new TomlTable(mergeFrom.TableType);

            foreach (var r in mergeFrom.Rows)
            {
                TomlObject toRowValue;
                if (mergeTo.Rows.TryGetValue(r.Key, out toRowValue))
                {
                    var ttTo = toRowValue as TomlTable;
                    var ttFrom = r.Value as TomlTable;
                    if (ttTo != null && ttFrom != null)
                    {
                        merged.Add(r.Key, ValuesOverwrittenFromTo(ttFrom, ttTo));
                    }
                    else
                    {
                        var copy = toRowValue.Clone(TomlObject.CloneModes.CloneValue);
                        copy.Comments = new List<TomlComment>(mergeFrom.Comments);
                        merged.Add(r.Key, copy);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
