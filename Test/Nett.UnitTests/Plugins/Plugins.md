The test cases here test the 'Plugin' use case scenario.

In such a case there is one main config where sub assemblies can register new sections 
in the main config. The nasty thing is, that the plugins should not need to 
take a dependency on the TOML library.

So the TOML library needs to expose enough API that the core config system
is able to map between TOML and generic CLR-structures without knowing how these
generic structures look like.

It gets really complicated, when the plugins also should be able to provide meaningful
comments that should be in the written config file. 