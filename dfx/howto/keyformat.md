# How to change key format for serialization

This section shows how to change the serialized TOML key format

```csharp
public class Root
{
    public int firstprop { get; set; } = 1;
    public int secondProp { get; set; } = 2;
    public int ThirdProp { get; set; } = 3;
    public int FOURTHPROP { get; set; } = 4;
}

var pascalSettings = TomlSettings.Create(s => s
    .ConfigurePropertyMapping(m => m
        .UseKeyGenerator(standardGenerators => standardGenerators.PascalCase)));

var upperSettings = TomlSettings.Create(s => s
    .ConfigurePropertyMapping(m => m
        .UseKeyGenerator(standardGenerators => standardGenerators.UperCase)));

var lowerSettings = TomlSettings.Create(s => s
    .ConfigurePropertyMapping(m => m
        .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

var camelSettings = TomlSettings.Create(s => s
    .ConfigurePropertyMapping(m => m
        .UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));

var defaultToml = Toml.WriteString(new Root());

var pascal = Toml.WriteString(new Root(), pascalSettings);

var upper = Toml.WriteString(new Root(), upperSettings);

var lower = Toml.WriteString(new Root(), lowerSettings);

var camel = Toml.WriteString(new Root(), camelSettings);
```

Default TOML:
```toml
firstprop = 1
secondProp = 2
ThirdProp = 3
FOURTHPROP = 4
```

Pascal:
```toml
Firstprop = 1
SecondProp = 2
ThirdProp = 3
FOURTHPROP = 4
```

Upper:
```toml
FIRSTPROP = 1
SECONDPROP = 2
THIRDPROP = 3
FOURHTPROP = 4
```

Lower:
```toml
firstprop = 1
secondprop = 2
thirdprop = 3
fourthprop = 4
```

Camel:
```toml
firstprop = 1
secondProp = 2
ThirdProp = 3
FOURTHPROP = 4
```

# How to ignore key format for property mapping

TOML is case sensitive. By default `Nett` will not map TOML values to
the corresponding object property if they are not exactly the same.

You can make Nett ignore the case of the target property by using 
the `IgnoreCase` target property selector:

```csharp
public class TestObject
{
    public string TestProp { get; set; }
}

// Create custom settings with 'IgnoreCase' property selector enabled
var settings = TomlSettings.Create(s => s
    .ConfigurePropertyMapping(m => m
        .UseTargetPropertySelector(standardSelectors => standardSelectors.IgnoreCase)));

const string TomlInput = "TestProp = 'x'";

// Deserialize TOML file content with default settings
var exact = Toml.ReadString<TestObject>(TomlInput);

// Deserialize TOML file content with custom settings enabled
var ignoreCase = Toml.ReadString<TestObject>(TomlInput, settings);

Debug.Assert(exact.TestProp == null);
Debug.Assert(ignoreCase.TestProp == "x");
```

Warning: If the `IgnoreCase` target property selector is used
and the input TOML has multiple keys that only differ in casing

```toml
testprop = 1
TestProp = 2
```

The mapping behavior is undefined.
