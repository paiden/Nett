using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class RowSegment : PathSegment
        {
            private readonly string key;

            public RowSegment(Type sourceType, string key)
                : base(sourceType)
            {
                this.key = key;
            }

            public override TomlObject Get(TomlObject target)
                => target switch
                {
                    TomlTable tbl => GetTableRowOrThrowOnNotFound(this.key, tbl),
                    _ => throw new InvalidOperationException("T1"),
                };

            public override TomlObject Resolve(TomlObject target, Func<TomlObject> resolveTargetsParent)
            {
                var item = target switch
                {
                    TomlTable tbl => GetTableRowOrCreateDefault(this.key, tbl, this.mappedType),
                    _ => throw new InvalidOperationException("T2"),
                };

                return item is TomlTable ? item : this.TryWrapInConvertProxy(item, resolveTargetsParent);
            }

            public override void Set(TomlObject target, Func<TomlObject, TomlObject> createNewValueObject)
            {
                if (target is TomlTable tbl)
                {
                    tbl.TryGetValue(this.key, out var cur);
                    tbl[this.key] = createNewValueObject(cur);
                }
                else
                {
                    throw new InvalidOperationException($"Cannot set '{this.key}' on object target '{target}'.");
                }
            }

            public override bool Clear(TomlObject target)
                => target switch
                {
                    TomlTable tbl => tbl.Remove(this.key),
                    _ => throw new InvalidOperationException("X4"),
                };

            public override string ToString() => $"/{this.key}";

            private TomlObject TryWrapInConvertProxy(TomlObject obj, Func<TomlObject> resolveObjsParent)
            {
                var conv = obj.Root.Settings.TryGetToTomlConverter(this.mappedType);
                if (conv != null)
                {
                    var convBack = obj.Root.Settings.TryGetConverter(obj.GetType(), this.mappedType);
                    if (convBack != null)
                    {
                        var instance = convBack.Convert(obj.Root, obj, this.mappedType);
                        var parent = (TomlTable)resolveObjsParent();
                        var proxy = new ConversionMappingTableProxy(
                            this.key, parent, instance.GetType(), obj.GetType(), Toml.Create(instance), conv);
                        return proxy;
                    }
                }

                return obj;
            }

            private class ConversionMappingTableProxy : TomlTable
            {
                private readonly string rowKey;
                private readonly Type clrObjectType;
                private readonly Type conversionTargetType;
                private readonly TomlTable parentTable;
                private readonly ITomlConverter converter;

                public ConversionMappingTableProxy(
                    string rowKey, TomlTable parentTable, Type clrObjectType, Type tomlType, TomlTable objTable, ITomlConverter converter)
                    : base(objTable.Root)
                {
                    this.parentTable = parentTable;
                    this.rowKey = rowKey;
                    this.converter = converter;
                    this.conversionTargetType = tomlType;
                    this.clrObjectType = clrObjectType;

                    foreach (var r in objTable.Rows)
                    {
                        var toAdd = r.Value is TomlTable t ? new ProxySubTable(t, this) : r.Value;
                        this.AddRow(new TomlKey(r.Key), toAdd);
                    }
                }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
                protected override void OnRowValueSet(string _)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
                {
                    this.UpdateProxiedTable();
                }

                private void UpdateProxiedTable()
                {
                    var obj = this.Get(this.clrObjectType);
                    this.parentTable[this.rowKey] =
                        (TomlObject)this.converter.Convert(this.parentTable.Root, obj, this.conversionTargetType);
                }

                private class ProxySubTable : TomlTable
                {
                    private ConversionMappingTableProxy root;

                    public ProxySubTable(TomlTable tbl, ConversionMappingTableProxy root)
                        : base(tbl.Root)
                    {
                        this.root = root;

                        foreach (var r in tbl)
                        {
                            var toAdd = r.Value is TomlTable t ? new ProxySubTable(t, this.root) : r.Value;
                            this.AddRow(new TomlKey(r.Key), toAdd);
                        }
                    }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
                    protected override void OnRowValueSet(string _)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
                    {
                        this.root.UpdateProxiedTable();
                    }
                }
            }
        }
    }
}
