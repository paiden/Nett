using System;

namespace Nett
{
    //public sealed class TomlValueWithUnit : TomlValue
    //{
    //    public TomlValueWithUnit(ITomlRoot root, TomlValue value, string unit)
    //        : base(root, value.UntypedValue)
    //    {
    //        this.Value = value;
    //        this.Unit = unit;
    //    }

    //    public TomlValue Value { get; }

    //    public string Unit { get; }

    //    public override string ReadableTypeName
    //        => "Value with Unit";

    //    public override TomlObjectType TomlType
    //        => TomlObjectType.ValueWithUnit;

    //    public override object Get(Type t)
    //    {
    //        if (t == Types.TomlValueWithUnit) { return this; }

    //        try
    //        {
    //            return base.Get(t);
    //        }
    //        catch
    //        {
    //            return this.Value.Get(t);
    //        }
    //    }

    //    public override void Visit(ITomlObjectVisitor visitor)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal override TomlObject CloneFor(ITomlRoot root)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal override TomlValue ValueWithRoot(ITomlRoot root)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    internal override TomlObject WithRoot(ITomlRoot root)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
