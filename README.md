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

All common TOML operations are performed via the static class 'Nett.Toml'. Although there are other
types available from the library in general using that single type should be sufficient
for most standard scenarios.

The following example shows how you can write and read some complex object to and from a
TOML file. The object that gets serialized and deserialized is defined as follows:

```
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

```
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
EnableDebug = false

[Server]
Timeout = 00:01:00


[Client]
ServerAddress = "http://127.0.0.1:8080"


```

To read that back into your object you need to:

```
var config = Toml.ReadFile<Configuration>("test.tml");
```

If you only have a TOML file but no corresponding class that the data in the TOML file
maps to, you can read the data into a generic TomlTable structure and extract a member
the like:

```
TomlTable table = Toml.ReadFile("test.tml");
var timeout = table.Get<TomlTable>("Server").Get<TimeSpan>("Timeout");
```

# Advanced Topics


# Changelog
[TBD] v1.0.0 (compatible with TOML 0.4.0) Initial Release


