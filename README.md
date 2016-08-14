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

Also for the `Write` operation a config object can be specified. But this configuration
will not get associated with the table. This config will only be used temporary 
during the write operation. If not config is specified for the write operation,
the associated config will be used to perform all write operations.

To create a new configuration do the following:
```C#
var myConfig = TomlConfig.Create();
```

This will create a copy of the default configuration. The default configuration can be modified
via a fluent configuration API.

The following sections will show this API can be used to support various use cases.

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

var config = Toml.ReadFile<ConfigurationWithDepdendency>(fn, myConfig);
```

## Allowing / disallowing implicit conversions between types

The default 'TOML' configuration allows a lot of implicit type conversions. These conversion are
provided by two distinct conversion sets that are activated by default. These sets are:

1. Cast  
   This cast set contains converters that mimic the .Net cast behavior for various types (e.g. TomlFloat and int).
   The casts include the implicit and explicit casts allowed in .Net.
2. Convert
   + Enum <-> TomlString
   + Guid <-> TomlString
   + Dictionary <-> TomlTable

   TODO

## Reading & writing non TOML types
'TOML' has a very limited set of supported types. Assume you have some very simple CLR type
used as the root table, that looks like:

```C#
public struct Money
{
    public string Currency { get; set; }
    public decimal Ammount { get; set; }

    public static Money Parse(string s) => new Money() { Ammount = decimal.Parse(s.Split(';')[0]), Currency = s.Split(';')[1] };
    public override string ToString() => $"{this.Ammount};{this.Currency}";
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
as a TomlFloat (double precision). 

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

Using this custom configuration will produce the following TOML which is what one would expect.
```
NotSupported = "9.99;EUR"
```

Also the deserialization will work because the conversion specified both directions (FromlToml & ToToml). It is not
required to always specify both conversion directions. E.g. if you only write TOML files, the 'FromToml' part
could be omitted.

# Changelog
2016-04-10: v0.4.2 (TOML 0.4.0)
 
+ Fix: Float was written as TomlInt when it had no decimal places [#8](https://github.com/paiden/Nett/issues/8)
+ Fix: Inline tables read as arrays [#7](https://github.com/paiden/Nett/issues/7)
+ Fix: Integer bare keys not working [#6](https://github.com/paiden/Nett/issues/6)

2016-02-14: v0.4.1 (compatible with TOML 0.4.0)

+ Add: Support for short date time formats
+ Fix: Writing files is culture invariant
+ Fix: Table encoding/decoding when they are used inside table arrays

2015-12-18: v0.4.0 (compatible with TOML 0.4.0) First public preview release


