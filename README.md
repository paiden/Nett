# What is Nett
Nett is a library that helps to read and write [TOML](https://github.com/toml-lang/toml) files in .Net.

# Objectives
+ Compatible (but not 100% compliant) with **latest** 'TOML' spec. All currently available TOML libs for C# (.Net) are pre 0.1.0 spec

# Differences to original TOML spec
* 'Nett' also allows you to specify timespan values. Timespan currently isn't supported in the original
'TOML' spec. The following TimeSpan formats are supported:

+ hh:mm
+ hh:mm:ss
+ hh:mm:ss.ff
+ dd.hh:mm:ss
+ dd.hh:mm:ss.ff

# Getting Started

Install it via NuGet:

|Package | Latest Version |
|--------|------|
|[Nett](https://www.nuget.org/packages/Nett/)| ![#](https://img.shields.io/nuget/v/Nett.svg)|
|[Nett Strong Named](https://www.nuget.org/packages/Nett.StrongNamed/)| ![#](https://img.shields.io/nuget/v/Nett.StrongNamed.svg)|

All common 'TOML' operations are performed via the static class `Nett.Toml`. Although there are other
types available from the library in general using that single type should be sufficient
for most standard scenarios.

The following example shows how you can write and read some complex object to/from a
'TOML' file. The object that gets serialized and deserialized is defined as follows:

```C#
public class Configuration
{
    public bool EnableDebug { get; set; }
    public Server Server { get; set; }
    public Client Client { get; set; }
}

public class Server
{
    public TimeSpan Timeout { get; set; }
}

public class Client
{
    public string ServerAddress { get; set; }
}
```

To write the above object to a 'TOML' File you have to do:

```C#
var config = new Configuration()
{
    EnableDebug = true,
    Server = new Server() { Timeout = TimeSpan.FromMinutes(1) },
    Client = new Client() { ServerAddress = "http://127.0.0.1:8080" },
};

Toml.WriteFile(config, "test.tml");
```

This will write the following content to your hard disk:

```
EnableDebug = true

[Server]
Timeout = 00:01:00


[Client]
ServerAddress = "http://127.0.0.1:8080"


```

To read that back into your object you need to:

```C#
var config = Toml.ReadFile<Configuration>("test.tml");
```

If you only have a 'TOML' file but no corresponding class that the data in the 'TOML' file
maps to, you can read the data into a generic TomlTable structure and extract a member
the like:

```C#
TomlTable table = Toml.ReadFile("test.tml");
var timeout = table.Get<TomlTable>("Server").Get<TimeSpan>("Timeout");
```

# Configuration

In advanced use cases 'Nett' behavior needs to be tweaked. This can be achieved by providing
custom configuration information. Currently there are two ways to modify the behavior of 
'Nett'.

1. Attributes that get applied on target objects and their properties  
2. A custom configuration object passed to Read/Write methods

A `TomlTable` is always associated with a configuration object. This association
is established via the `Read` and `Create` methods. If no config is specified 
during a read/create the default config will be used. There are overloads of
these methods that allow to associate a custom configuration. Once a table 
is associated with a custom configuration this association cannot be changed
for that table instance.

Also for the `Write` operation a config object can be specified. But, that configuration
will not get associated with the table. It will only be used temporary 
during the write operation. If no config is specified for the write operation,
the table associated config will be used to perform all write operations.

To create a new configuration do the following:
```C#
var myConfig = TomlConfig.Create();
```

This will create a copy of the default configuration. The copy can be modified
via a fluent configuration API.

The following sections will show how this API can be used to support various use cases.

## Deserializing types without default constructor
If your type doesn't have a default constructor or is not constructible (interface or abstract
class) 'Nett' will not be able to deserialize
into that type without some help.

Assume we have the following type, that extends the configuration class from the basic
examples:

```C#
public class ConfigurationWithDepdendency : Configuration
{
    public ConfigurationWithDepdendency(object dependency)
    {

    }
}
```

When you try to deserialize the `test.tml` into that type via

```C#
var config = Toml.ReadFile<ConfigurationWithDepdendency>("test.tml");
```

you will get the following exception:

`Failed to create type 'ConfigurationWithDepdendency'. Only types with a parameterless constructor or an
specialized creator can be created. Make sure the type has a parameterless constructor or a
configuration with an corresponding creator is provided.`

To make this work, we need to pass a custom configuration to the read method that tells 'Nett', how
the type can be created. This is done the by:

```C#
var myConfig = TomlConfig.Create(cfg => cfg
    .ConfigureType<ConfigurationWithDepdendency>(ct => ct
        .CreateInstance(() => new ConfigurationWithDepdendency(new object()))));

var config = Toml.ReadFile<ConfigurationWithDepdendency>("test.tml", myConfig);
```

## Allowing / disallowing implicit conversions between types

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
var config = TomlConfig.Create(cfg => cfg
    .AllowImplicitConversions(TomlConfig.ConversionSets.All));
var tbl = Toml.ReadString("f = 0.99", config);
var i = tbl.Get<int>("f"); // i will be '0'
```

This example shows the drawbacks of activating all conversions. Here the read `int`
will have a value of 0. The next write would write value `0` into the TOML file and
so probably change the type of the config value. Simply explained, the more conversion are 
enabled, the higher the risk is that subtle bugs are introduced.

The opposite route is to disable all 'Nett' implicit conversion via 

```C#
var config = TomlConfig.Create(cfg => cfg
    .AllowImplicitConversions(TomlConfig.ConversionSets.None));
var tbl = Toml.ReadString("i = 1", config);
// var i = tbl.Get<int>("i"); // Would throw InvalidOperationException as event cast from TomlInt to int is not allowed
var i = tbl.Get<long>("i"); // Only long will work, no other type
```

The drawback of this approach is that your objects are only allowed to use TOML native types to work without further 
casting or custom converters.

Any set combination can be activated by logical combination of the set flags e.g.:

```C#
var config = TomlConfig.Create(cfg => cfg
    .AllowImplicitConversions(TomlConfig.ConversionSets.NumericalType | TomlConfig.ConversionSets.Serialize));
```

Var various scenarios a logical combination of the default conversion sets with some custom converters may
be the best choice.

## Handle non TOML types via custom converters
'TOML' has a very limited set of supported types. Assume you have some very simple CLR type
used for config root object called `TableContainingMoney`:

```C#
public struct Money
{
    public string Currency { get; set; }
    public decimal Ammount { get; set; }

    public static Money Parse(string s) => new Money() { Ammount = decimal.Parse(s.Split(' ')[0]), Currency = s.Split(' ')[1] };
    public override string ToString() => $"{this.Ammount} {this.Currency}";
}

public class TableContainingMoney
{
    public Money NotSupported { get; set; }
}
```

With the default configuration 'Nett' would produce the following 'TOML' content
```

[NotSupported]
Currency = "EUR"

[NotSupported.Ammount]

```

This not very useful content is generated because of

1. 'Nett' treats Money as a complex type and therefore writes it as a table
2. 'Nett' cannot handle the `decimal` type by default.

Reading back this generated 'TOML' will not produce the same data structure 
as during the write.

To fix this we can try to tell Nett how to handle the `decimal` type correctly.
In this use case we decide we don't care about precision and just write is
as a TomlFloat that has double precision. 

```C#
var obj = new TableContainingMoney()
{
    NotSupported = new Money() { Ammount = 9.99m, Currency = "EUR" }
};

var config = TomlConfig.Create(cfg => cfg
    .ConfigureType<decimal>(type => type
        .WithConversionFor<TomlFloat>(convert => convert
            .ToToml(dec => (double)dec)
            .FromToml(tf => (decimal)tf.Value))));

var s = Toml.WriteString(obj, config);
var read = Toml.ReadString<TableContainingMoney>(s, config);
```

Now 'Nett' will produce the following output:

```

[NotSupported]
Currency = "EUR"
Ammount = 9.99
```

This is already a lot better and will read back the correct data structure.
But in our case the money type itself can serialize it to a string. So it's 
functionality can be used to store the information more efficiently and not as 
a TomlTable (complex data structure) by using 
a different converter telling 'Nett' how to handle the 'Money' type itself rather
than it's components.

```C#
var obj = new TableContainingMoney()
{
    NotSupported = new Money() { Ammount = 9.99m, Currency = "EUR" }
};

var config = TomlConfig.Create(cfg => cfg
    .ConfigureType<Money>(type => type
        .WithConversionFor<TomlString>(convert => convert
            .ToToml(custom => custom.ToString())
            .FromToml(tmlString => Money.Parse(tmlString.Value)))));

var s = Toml.WriteString(obj, config);
var read = Toml.ReadString<TableContainingMoney>(s, config);
```

Using this custom configuration will produce the following TOML which is more efficient and readable.
```
NotSupported = "9.99 EUR"
```

Also the deserialization will work because the conversion specified both directions (FromlToml & ToToml). It is not
required to always specify both conversion directions. E.g. if you only write TOML files, the 'FromToml' part
could be omitted.

# Changelog
2016-08-14: **v0.5.0** *(TOML 0.4.0)*

+ Changed: Configuration API to have clearer syntax and behavior
+ Add: implicit cast sets; Guids and Enums are converted automatically
+ Fix: Weird formatting and new lines for nested tables
+ Fix: Invalid TOML strings produce better parser error message

2016-04-10: **v0.4.2** *(TOML 0.4.0)*
 
+ Fix: Float was written as TomlInt when it had no decimal places [#8](https://github.com/paiden/Nett/issues/8)
+ Fix: Inline tables read as arrays [#7](https://github.com/paiden/Nett/issues/7)
+ Fix: Integer bare keys not working [#6](https://github.com/paiden/Nett/issues/6)

2016-02-14: **v0.4.1** *(TOML 0.4.0)**

+ Add: Support for short date time formats
+ Fix: Writing files is culture invariant
+ Fix: Table encoding/decoding when they are used inside table arrays

2015-12-18: **v0.4.0** (compatible with TOML 0.4.0) First public preview release


