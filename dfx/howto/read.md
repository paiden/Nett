# TOML to class
TOML input file:
```toml
EnableDebug = true

[Server]
Timeout = 1m

[Client]
ServerAddress = "http://127.0.0.1:8080"
```

Code:
```csharp
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

// ...

var cust = Toml.ReadFile<Configuration>(filename);

Console.WriteLine("EnableDebug: " + cust.EnableDebug);
Console.WriteLine("Timeout: " + cust.Server.Timeout);
Console.WriteLine("ServerAddress: " + cust.Client.ServerAddress);
```

Output:
```
EnableDebug: True
Timeout: 00:01:00
ServerAddress: http://127.0.0.1:8080
```

The properties of the object have to
+ be public
+ match the TOML key casing
+ expose a public setter

Advanced &lt;behavior configuration&gt; allows you to tweak these requirements.

# TOML to TomlTable

TOML input file:v
```toml
EnableDebug = true

[Server]
Timeout = 1m

[Client]
ServerAddress = "http://127.0.0.1:8080"
```

Code:
```csharp
var toml = Toml.ReadFile(filename);
Console.WriteLine("EnableDebug: " + toml.Get<bool>("EnableDebug"));
Console.WriteLine("Timeout: " + toml.Get<TomlTable>("Server").Get<TimeSpan>("Timeout"));
Console.WriteLine("ServerAddress: " + toml.Get<TomlTable>("Client").Get<string>("ServerAddress"));
```

Output:
```
EnableDebug: True
Timeout: 00:01:00
ServerAddress: http://127.0.0.1:8080
```
`TomlTable` is `Nett's` generic representation of a TomlDocument. It is 
a hash set based data structure where each key is 
represented as a `string` and each value as a `TomlObject`.

Using the `TomlTable` representation has the benefit of having TOML
metadata - e.g. the Comments - available in the data model.

# TOML to Dictionary
TOML input file:
```toml
EnableDebug = true

[Server]
Timeout = 1m

[Client]
ServerAddress = ""http://127.0.0.1:8080""
```

Code: 
```csharp
var data = Toml.ReadFile(filename).ToDictionary();
var server = (Dictionary<string, object>)data["Server"];
var client = (Dictionary<string, object>)data["Client"];

Console.WriteLine("EnableDebug: " + data["EnableDebug"]);
Console.WriteLine("Timeout: " + server["Timeout"]);
Console.WriteLine("ServerAddress: " + client["ServerAddress"]);
```

Output:
```
EnableDebug: True
Timeout: 00:01:00
ServerAddress: http://127.0.0.1:8080
```

With `ToDictionary()` the data can be transformed to a standard 
`Dictionary<string, object>` representation. 
