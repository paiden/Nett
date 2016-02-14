# What is Nett
Nett is a library that helps to read and write [TOML](https://github.com/toml-lang/toml) files in .Net.

# Objectives
+ Compatible (but not 100% compliant) with **latest** TOML spec. All currently available TOML libs for C# (.Net) are pre 0.1.0 spec

# Differences to original TOML spec
* Nett also allows you to specify timespan values. Timespan currently isn't supported in the original
TOML spec. The following TimeSpan formats are supported:

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

All common TOML operations are performed via the static class 'Nett.Toml'. Although there are other
types available from the library in general using that single type should be sufficient
for most standard scenarios.

The following example shows how you can write and read some complex object to/from a
TOML file. The object that gets serialized and deserialized is defined as follows:

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

To write the above object to a TOML File you have to do:

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

If you only have a TOML file but no corresponding class that the data in the TOML file
maps to, you can read the data into a generic TomlTable structure and extract a member
the like:

```C#
TomlTable table = Toml.ReadFile("test.tml");
var timeout = table.Get<TomlTable>("Server").Get<TimeSpan>("Timeout");
```

# Advanced Topics
All advanced concepts of the TOML serialization and deserialization process can be controlled
via C# attributes or providing a custom TomlConfiguration instance when invoking the Read/Write
methods.

To create a new configuration do the following:
```C#
var myConfig = TomlConfig.Create();
```

This will create a copy of the default configuration, and you can configure this new copy
via builder methods that will be described in more detail in the next topics.

## Deserializing types without default constructor
If your type doesn't have a default constructor or is not constructable (interface or abstract
class) Nett will not be able to deserialize
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

When you try to deserialze the `test.tml` into that type via

```C#
var config = Toml.ReadFile<ConfigurationWithDepdendency>("test.tml");
```

you will get the following exception:

`Failed to create type 'ConfigurationWithDepdendency'. Only types with a parameterless constructor or an
specialized creator can be created. Make sure the type has a parameterless constructor or a
configuration with an corresponding creator is provided.`

To make this work, we need to pass a custom configuration to the read method that tells Nett, how
the type can be created. This is done the by:

```C#
var myConfig = TomlConfig.Create()
    .ConfigureType<ConfigurationWithDepdendency>()
        .As.CreateWith(() => new ConfigurationWithDepdendency(new object()))
    .Apply();

var config = Toml.ReadFile<ConfigurationWithDepdendency>("test.tml", myConfig);
```

## Reading & writing non TOML types
TOML has a very limited set of supported types. So how can non supported types e.g.
System.Guid be used in a object that has to be written to, or read from TOML?

To support such scenarios you can register custom type converters for a type that
tell Nett how a type can be converted from some arbitrary type to TOML vice versa.
The following example shows how you register type converters for both reading and
writing a GUID as TOML (you only need to register a converter for the operation you
use):

```C#
var obj = new TypeNotSupportedByToml() { SomeGuid = new Guid("6836AA79-AC1C-4173-8C58-0DE1791C8606") };

var myconfig = TomlConfig.Create()
    .ConfigureType<Guid>()
        .As.ConvertTo<TomlString>().As((g) => new TomlString(g.ToString()))
        .And.ConvertFrom<TomlString>().As((s) => new Guid(s.Value))
    .Apply();

Toml.WriteFile(obj, "test.tml", myconfig);
var read = Toml.ReadFile<TypeNotSupportedByToml>("test.tml", myconfig);
```

Note: If you execute the above code without type converters the code will not throw any exception.
Instead you will get an empty Guid. This behavior is caused by the fact, that during writes every
unknown type is treated as a TOML table and during read a TOML table is handled as a type that
can be converted to any other type.
Hopefully this somewhat strange behavior will improve in a future version of Nett.

# Changelog
2016-02-14: v0.4.1 (compatible with TOML 0.4.0)

+ Add: Support for short date time formats
+ Fix: Writing files is culture invariant
+ Fix: Table encoding/decoding when they are used inside table arrays

2015-12-18: v0.4.0 (compatible with TOML 0.4.0) First public preview release


