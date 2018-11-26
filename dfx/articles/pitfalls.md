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