# Introduction
TODO

# DateTime vs DateTimeOffset

.Net developers are often used to use DateTime data type by default.

```csharp 
class Foo { public DateTime TimteStamp {get;set;}}
```

Nett will convert DateTimes to DateTimeOffsets internally.
This leads to the followng issue outlined by a code example.

```csharp
var data = new Foo { Timestamp = DateTime.Now };
var written = Toml.WriteString(subject);
var readBack = Toml.ReadString(written);

// original datetime: 2018-01-01T22:25:32.010354+01:00
// read back datetime: 2018-01-01T21:25:32.010354+00:00
```

The problem here is that the read back DateTime will be 
different from the original DateTime. The difference depends on
the time zone you are located in.

This is not a bug but simply a loss of information when 
the DateTimeOffset - Nett uses in the TOML object to store the
information - is converted to/from a DateTime object.

When converting a DateTimOffset `dto` to a DateTime there are 
two possible ways to do so

1. ```DateTime dt = dto.UtcDateTime`
2. ```DateTime dt = dto.LocalDateTime`

Nett has not idea what kind of DateTime is expected by the
class object, so by default it does the `UtcDateTime` conversion.

If the data object expects `LocalDateTime` this will lead to 
time differences between serialization and deserialization.

# Property mapping issues

When mapping a TOML table to a .Net object there can be various
reasons, why this mapping does not work as expected. This section
of the documentation tries to explain the mechanisms 'Nett' has 
in place to help to diagnose such issues.

## On target property missing callback
This is a callback that will get invoked when Nett tries to map
a TOML table row to a corresponding property in a .Net object 
but cannot find a suitable property on the object. 

Users can install custom callbacks and implement whatever error
action is desired (including throwing exceptions).

```csharp
var settings = TomlSettings.Create(s => s
    .ConfigurePropertyMapping(m => m
        .OnTargetPropertyNotFound((k, t, v) => { key = k; tgt = t; val = v; })));
```

The callback tries to give as much information as possible to allow
the user to provide as good error messages as possible.

The first argument is an array of string that represent all TOML
keys until the row that should get mapped. So if you have try to map
the TOML row a.b.c the first argument will be `["a", "b", "c"]`.

The second argument gives you the instance of the target object. 

The third argument is the TOML value object that could not be mapped.
