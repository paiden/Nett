The TOML table will contain rows mapped from CLR types & objects
to TOML primitives. This mapping will be done for all public 
properties of the object.

Properties can be ignored via the @"Nett.TomlIgnoreAttribute" or 
using a accordingly set up @"Nett.TomlConfig" to create
the table.