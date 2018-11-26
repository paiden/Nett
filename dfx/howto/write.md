
# Class to TOML
Code:
```csharp
public class Configuration
{
    public bool EnableDebug { get; set; }
    public Server Server { get; set; } = new Server();
    public Client Client { get; set; } = new Client();
}

public class Server
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}

public class Client
{
    public string ServerAddress { get; set; } = "http://localhost:8082";
}

...

var obj = new Configuration();
Toml.WriteFile(obj, filename);
```

Written TOML file:
```toml
EnableDebug = false

[Server]
Timeout = 2m

[Client]
ServerAddress = "http://localhost:8082"
```

The properties of the object have to
+ be public
+ expose a public getter

# TomlTable to TOML
Code:
```csharp
var server = Toml.Create();
server.Add("Timeout", TimeSpan.FromMinutes(2));

var client = Toml.Create();
client.Add("ServerAddress", "http://localhost:8082");

var tbl = Toml.Create();
tbl.Add("EnableDebug", false);
tbl.Add("Server", server);
tbl.Add("Client", client);

Toml.WriteFile(tbl, filename);
```
Written TOML file:
```toml
EnableDebug = false

[Server]
Timeout = 2m

[Client]
ServerAddress = "http://localhost:8082"
```

# Dictionary to TOML

Code:
```csharp
var data = new Dictionary<string, object>()
{
    { "EnableDebug", false },
    { "Server", new Dictionary<string, object>() { { "Timeout", TimeSpan.FromMinutes(2) } } },
    { "Client", new Dictionary<string, object>() { { "ServerAddress", "http://localhost:8082" } } },
};

Toml.WriteFile(data, filename);
```
Written TOML file:
```toml
EnableDebug = false

[Server]
Timeout = 2m

[Client]
ServerAddress = "http://localhost:8082"
```