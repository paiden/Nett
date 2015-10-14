using System.Diagnostics;

namespace Nett
{
    internal static class TomlTableTests
    {
        internal static TomlTable ValuesOverwrittenFromTo(TomlTable mergeFrom, TomlTable mergeTo)
        {
            Debug.Assert(mergeFrom != null);
            Debug.Assert(mergeTo != null);

            var merged = (TomlTable)mergeTo.Clone(TomlObject.CloneModes.CloneValue);

            foreach (var r in mergeFrom.Rows)
            {
                TomlObject existing;
                if (merged.Rows.TryGetValue(r.Key, out existing))
                {
                    var mergedValue = existing.Clone(TomlObject.CloneModes.CloneValue);
                    mergedValue.Comments.AddRange(existing.Comments);
                    merged[r.Key] = mergedValue;
                }
                else
                {
                    merged[r.Key] = r.Value;
                }
            }

            return merged;
        }
    }
}
