# Configuration

In advanced use cases 'Nett' behavior needs to be tweaked. This can be achieved by providing
custom configuration information. Currently there are two ways to modify the behavior of 
'Nett'.

1. Attributes that get applied on target objects and their properties  
2. A custom configuration object passed to Read/Write methods

A `TomlTable` is always associated with a configuration object. This association
is established via the `Read` and `Create` methods. If no config is specified 
during a read/create the default config will be used. There are overloads of
these methods that allow to associate a custom configuration. Once a table 
is associated with a custom configuration this association cannot be changed
for that table instance.

Also for the `Write` operation a config object can be specified. But, that configuration
will not get associated with the table. It will only be used temporary 
during the write operation. If no config is specified for the write operation,
the table associated config will be used to perform all write operations.

To create a new configuration do the following:
```C#
var myConfig = TomlSettings.Create();
```

This will create a copy of the default configuration. The copy can be modified
via a fluent configuration API.

The following sections will show how this API can be used to support various use cases.