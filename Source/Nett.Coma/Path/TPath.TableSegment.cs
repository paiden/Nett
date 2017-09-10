using System;
using System.Diagnostics;
using Nett.Extensions;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class TableSegment : KeySegment
        {
            private readonly Type clrType;

            public TableSegment(string key, Type clrType)
                : base(TomlObjectType.Table, key)
            {
                this.clrType = clrType.CheckNotNull(nameof(clrType));
            }

            public override TomlObject Apply(
                TomlObject obj, Func<TomlObject> resolveParent, PathSettings settings = PathSettings.None)
            {
                var item = base.TryApply(obj, resolveParent, settings)
                    ?? this.TryCreateTable(obj, settings)
                    ?? throw new InvalidOperationException(this.KeyNotFoundMessage);

                return item.TomlType != TomlObjectType.Table ? this.TryWrapInConvertProxy(item, resolveParent) : item;
            }

            public override TomlObject TryApply(
                TomlObject obj, Func<TomlObject> resolveParent, PathSettings settings = PathSettings.None)
            {
                try
                {
                    return base.TryApply(obj, resolveParent, settings) ?? this.TryCreateTable(obj, settings);
                }
                catch
                {
                    Debug.Assert(false, "This should never happen if the stuff in the try block is implemented correctly.");
                    return null;
                }
            }

            public override string ToString() => $"/{{{this.key}}}";

            private TomlTable TryCreateTable(TomlObject obj, PathSettings settings)
            {
                if (settings.HasFlag(PathSettings.CreateTables) && obj is TomlTable tbl)
                {
                    var table = tbl.AddTomlObject(this.key, tbl.CreateEmptyAttachedTable());
                    return table;
                }

                return null;
            }

            private TomlObject TryWrapInConvertProxy(TomlObject obj, Func<TomlObject> resolveParent)
            {
                var conv = obj.Root.Settings.TryGetToTomlConverter(this.clrType);
                if (conv != null)
                {
                    var convBack = obj.Root.Settings.TryGetConverter(obj.GetType(), this.clrType);
                    if (convBack != null)
                    {
                        var instance = convBack.Convert(obj.Root, obj, this.clrType);
                        var parentTable = (TomlTable)resolveParent();
                        var proxy = new ConversionMappingTableProxy(
                            this.key, parentTable, instance.GetType(), obj.GetType(), Toml.Create(instance), conv);
                        return proxy;
                    }
                }

                return obj;
            }
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
