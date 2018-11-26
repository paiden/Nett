## Ignore CLR object properties

By default `Nett` reads/writes all public properties of an object. In some cases
it may be required change that for various reasons.

The following class outlines such a scenario

```csharp
public sealed class Computed
{
    public int X { get; set; } = 1;
    public int Y { get; set; } = 2;
    
    public int Z => X + Y;
}
```

Serializing an instance of this class will produce the following TOML content

```
X = 1
Y = 2
Z = 3
```

The instance is serializeable but deserializing this content into a Computed Instance will
fail with and message that will be something like `Property set method not found`, because
the computed property only has a getter, but no setter.

The fluent configuration API allows to ignore a property of an type.

```csharp
var c = new Computed();

// Create settings that will ignore the 'Z' propoerty of type 'Computed'
var settings = TomlSettings.Create(cfg => cfg
    .ConfigureType<Computed>(type => type
        .IgnoreProperty(o => o.Z)));

var w = Toml.WriteString(c, settings);

// This read operation succeeds
var r = Toml.ReadString<Computed>(w, settings);

// This read operation fails
var r = Toml.ReadString<Computed>(w);
```