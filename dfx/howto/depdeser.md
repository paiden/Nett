# How to deserialize types that have a constructor dependency

If a type doesn't have a default constructor or is not constructible (interface or abstract
class) 'Nett' will not be able to deserialize to that type without some help.

Such a type could look semothing like this:

```csharp
public class ConfigurationWithDepdendency : Configuration
{
    public ConfigurationWithDepdendency(object dependency)
    {

    }
}
```

When trying to deserialize a TOML document:

```csharp
var config = Toml.ReadString<ConfigurationWithDepdendency>("");
```

the following exception will be thrown:

`Failed to create type 'ConfigurationWithDepdendency'. Only types with a parameterless constructor or an
specialized creator can be created. Make sure the type has a parameterless constructor or a
configuration with an corresponding creator is provided.`

To make this work, we need to pass a custom configuration to the read method that tells 'Nett', how
the type can be created. This is done the by:

```csharp
var myConfig = TomlSettings.Create(cfg => cfg
    .ConfigureType<ConfigurationWithDepdendency>(ct => ct
        .CreateInstance(() => new ConfigurationWithDepdendency(new object()))));

var config = Toml.ReadFile<ConfigurationWithDepdendency>("test.tml", myConfig);
```
