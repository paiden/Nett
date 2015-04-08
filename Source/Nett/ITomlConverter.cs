using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public interface ITomlConverter
    {
        Type TargetType { get; }
        Type SourceType { get; }

        TomlObject ToToml(object value);
        object FromToml(object value);
    }

    public interface ITomlConverter<TTarget, TSource> : ITomlConverter
    {
        TomlObject ToToml(TTarget value);
        TTarget FromToml(TSource value);
    }
}
