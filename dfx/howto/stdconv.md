# Eanble/Disable implicit conversions between types

'Nett' defines the following standard conversion sets that be activated/deactivated via a 'TOML' config.

1. NumericalSize  
   Only conversions between floating point and integral data types are disallowed. All other conversions are 
   allowed, also the ones where the target type could be to small to hold the source value e.g. TomlInt -> char.
2. Serialize
   + Enum <-> TomlString
   + Guid <-> TomlString
3. NumericalType  
   Also allow conversion between floating point and integral data types e.g. TomlFloat -> char.

By **default** the *'NumericalSize'* and *'Serialize'* sets are activated. All possible conversions that Nett can do 
can be activated by:

```C#
var config = TomlSettings.Create(cfg => cfg
    .AllowImplicitConversions(TomlSettings.ConversionSets.All));
var tbl = Toml.ReadString("f = 0.99", config);
var i = tbl.Get<int>("f"); // i will be '0'
```

This example shows the drawbacks of activating all conversions. Here the read `int`
will have a value of 0. The next write would write value `0` into the TOML file and
so probably change the type of the config value. Simply explained, the more conversion are 
enabled, the higher the risk is that subtle bugs are introduced.

The opposite route is to disable all 'Nett' implicit conversion via 

```C#
var config = TomlSettings.Create(cfg => cfg
    .AllowImplicitConversions(TomlSettings.ConversionSets.None));
var tbl = Toml.ReadString("i = 1", config);
// var i = tbl.Get<int>("i"); // Would throw InvalidOperationException as event cast from TomlInt to int is not allowed
var i = tbl.Get<long>("i"); // Only long will work, no other type
```

The drawback of this approach is that your objects are only allowed to use TOML native types to work without further 
casting or custom converters.

Any set combination can be activated by logical combination of the set flags e.g.:

```C#
var config = TomlSettings.Create(cfg => cfg
    .AllowImplicitConversions(TomlSettings.ConversionSets.NumericalType | TomlSettings.ConversionSets.Serialize));
```

Var various scenarios a logical combination of the default conversion sets with some custom converters may
be the best choice.