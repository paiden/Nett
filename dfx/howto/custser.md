# How to use custom type serialization

When a type has provides custom serialization logic:

```csharp


public struct Money
{
    public string Currency { get; set; }
    public double Ammount { get; set; }

    public static Money Parse(string s) => new Money() { Ammount = double.Parse(s.Split(' ')[0]), Currency = s.Split(' ')[1] };
    public override string ToString() => $"{this.Ammount} {this.Currency}";
}
```

Nett can be configured to leverage this custom serialization logic by:

```csharp 
var obj = new RootTable()
{
    ToPay = new Money() { Ammount = 9.99, Currency = "EUR" }
};

var config = TomlSettings.Create(cfg => cfg
    .ConfigureType<Money>(type => type
        .WithConversionFor<TomlString>(convert => convert
            .ToToml(custom => custom.ToString())
            .FromToml(tmlString => Money.Parse(tmlString.Value)))));

var s = Toml.WriteString(obj, config);
```

This will generate the following TOML output:

```toml
ToPay = "9.99 EUR"
```

instead of the default output that would write the Money type as a TOML table:

```toml
[ToPay]
Ammout = 9.99
Currency = "EUR"
```