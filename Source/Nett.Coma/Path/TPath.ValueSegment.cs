using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private class ValueSegment : KeySegment
        {
            public ValueSegment(TomlObjectType segmentType, string key)
                : base(segmentType, key)
            {
                if (segmentType > TomlObjectType.TimeSpan)
                {
                    throw new ArgumentException(
                        $"The passed TOML type '{segmentType}' is invalid because it is not a TOML value type.");
                }
            }

            public override TomlObject Apply(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override TomlObject ApplyOrCreate(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public override TomlObject TryApply(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override string ToString() => $"/{this.key}";
        }
    }
}
