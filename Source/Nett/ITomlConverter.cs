using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public interface ITomlConverter
    {
        Type FromType { get; }
        Type ToType { get; }

        object Convert(object value);
    }

    public interface ITomlConverter<TFrom, TTo> : ITomlConverter
    {
        TTo Convert(TFrom src);
    }
}
