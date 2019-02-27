# Units

The 'Unit' experimental feature is a simplified version 
of a feature discussed in [#603](https://github.com/toml-lang/toml/issues/603)

The unit is a TomlValue meta data property to ease the processing
of input data.

Instead of encoding simple values with an unit as strings e.g.

```
money = "100.0 EUR"
```

it can be encoded into a new constructor that has the form

```
key = <TOML value> ( <unit> )
```

So the improved TOML for the previous example will be

```toml
key = 100.0 (EUR)
```

The following example shows how this feature can be used
in to read (write) this enhanced TOML.

```csharp
public class Cfg 
{
    public Money MyMoney {get;set;} = new Money();
}

public class Money
{
    public double Value { get; set; } = 10.2;

    public string Currency { get; set; } = "EUR";
}

var cfg = TomlSettings.Create(s => s
    .EnableExperimentalFeatures(f => f
        .ValuesWithUnit())
    .ConfigureType<Money>(ct => ct
        .WithConversionFor<TomlFloat>(conv => conv
            .ToToml(m => m.Value, m => m.Currency)
            .FromToml(uv => new Money() 
                { 
                    Currency = uv.GetUnit(), 
                    Value = uv.Value 
                }))));

const string tml = "MyMoney = 120.0 ($)"

var r = Toml.ReadString<Cfg>(tml, cfg);
Debug.Assert(r.MyMoney.Value == 120.0);
Debyg.Assert(r.MyMoney.Currency == "$");

var o = Toml.ReadString(tml, cfg);
var f = o.Get<TomlFloat>("MyMoney");
Debug.Assert(f.Value == 120.0);
Debyg.Assert(f.GetUnit() == "$");
```

Only the strings between the '(' and ')' chars get removed. 
All other characters including any whitespace character in the
unit are currently preserved. There is no escaping mechanism for
units, so unit cannot contain the '(' or ')' character.

A few examples on what TOML fragments will produce what
.Net string values for the corresponding unit.

| TOML              | .Net string value |
|-------------------|-------------------|
| X = 1 ()          | ""                |
| x = 2 (A)         | "A"               |
| x = 2 (3)         | "3"               |
| x = 2 ( 3 A   )   | "3 A"             |
| x = 2 (m \ s)     | "m \ s"           |
| x = 2 (m\s)       | "m\s"             |
| x = 2 (   m\s   ) | "m\s"             |
| x = 2 ("B")       | "\"B\""           |

Note: `x=2(m\s)` and `x=2(m \ s)` do **not** have the same unit,
wile `x=2(m\s)` and `x=2(    m\s    )` do.