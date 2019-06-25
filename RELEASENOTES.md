## v0.13.0
Nett:
+ Add: Callback for processing non map-able table rows [#78](https://github.com/paiden/Nett/issues/78)

## v0.12.0
Nett:
+ Fix: ArgumentOutOfRange exception when DateTime field/prop not initialized [#25](https://github.com/paiden/Nett/issues/25)
+ Fix: Improve conversion error messages [#29](https://github.com/paiden/Nett/issues/29)

Coma: 
+ Add: New overload for `Get(...)` that allows to specify a default value [#74](https://github.com/paiden/Nett/issues/74)

AspNet:
+ Add: Support for array of tables and jagged arrays conversion [#75](https://github.com/paiden/Nett/issues/75)
+ Fix: Fix missing dependency for NuGet package [#72](https://github.com/paiden/Nett/issues/72)

Exp:
+ Add: Experimental features project & package
+ Add: Values with unit experimental feature

## v0.11.0
Nett: 

+ Add: API to map object properties to specific TOML keys [#69](https://github.com/paiden/Nett/issues/69)
+ Refactor: **[Breaking Change]** Make TOML object factory API fluent [#68](https://github.com/paiden/Nett/issues/68)

AspNet:

+ Fix: 'Unexpected' exception when loading unsupported TOML types [#38](https://github.com/paiden/Nett/issues/38)
+ Fix: Deeper nested config objects loaded correctly [#71](https://github.com/paiden/Nett/issues/71)

## v0.10.1
Nett: 
+ Add: TomlObjects implement `ToString()` [#63](https://github.com/paiden/Nett/issues/51)
+ Fix: `Get<object>` automatically maps to equivalent CLR type [#63](https://github.com/paiden/Nett/issues/51)
+ Fix: DateTime Read/Write format corruption [#66](https://github.com/paiden/Nett/issues/66)

## v0.10.0

Nett:
+ Nett is now TOML v0.5.0 compliant
+ NuGet packages are strong named by default, '.StrongName' versions of packages are obsolete.
+ Refactor: New lexer and parser (improved performance)
+ Fix: Inline tables serialized in wrong container [#51](https://github.com/paiden/Nett/issues/51)

## v0.9.0

Nett:

+ **Breaking** Change: Rename TomlTimeSpan to TomlDuration
+ **Breaking** Change: TomlDuration uses Go-Like duration format as described in [Toml/#514](https://github.com/toml-lang/toml/issues/514)
+ Fix: Updating of TomlTables with TableArrayTypes [#44](https://github.com/paiden/Nett/issues/44)
+ Fix: Table rows written into wrong section [#42](https://github.com/paiden/Nett/issues/42)
+ Fix: NotImplementedException when using table arrays [#41](https://github.com/paiden/Nett/issues/41)

## v0.8.0

General: 

+ Add: .Net Standard 2.0 support
+ Add: Nett.AspNet package that integrates TOML into the Asp.Net Core configuration system
+ **Breaking** Change: API changes creating / adding TOML objects in generic TOML
+ Removed: Strong named packages

Nett:

+ Add: API for updating a TomlTable row
+ Add: Dictionary can be serialized directly

Coma:

+ Add: API to use custom store implementations for configurations.
+ Add: API for combining TOML tables
+ Change: The coma configuration API 

## v0.7.0

Nett: 
+ Add: Factory methods to allow generic construction of TOML object graphs [#21](https://github.com/paiden/Nett/issues/21)
+ Change: Rename Nett internal settings object from 'TomlConfig' to 'TomlSettings'
+ Fix: Write key back to file with same type [#23](https://github.com/paiden/Nett/issues/23)
+ Fix: Parse errors caused by absent optional whitespace [#26](https://github.com/paiden/Nett/issues/26)
+ Fix: Various TOML array parser issues

Coma:
+ Add: Settings can be moved between config scopes.

## v0.6.3

Nett:
+ Fix: Serialize `uint` correctly [#16](https://github.com/paiden/Nett/issues/16)

Coma:
+ Fix: Type conversions  [#17](https://github.com/paiden/Nett/issues/17)

## v0.6.2
Nett:
+ Fix: Ignore static properties [#15](https://github.com/paiden/Nett/issues/15)

## v0.6.1
Nett:
+ Fix: Array of tables serialization [#14](https://github.com/paiden/Nett/issues/14)

## v0.6.0

Nett:
+ Add: Properties of TOML mapped classes can be ignored via attribute or config
+ Add: TomlTable supports Freezable pattern
+ Fix: All parser errors include line and column
+ Fix: Various invalid TOML cases now cause a parser error as expected
+ Removed: Comments merge mode (will be redesigned and added in future version)

Coma: 
+ Initial release


## v0.5.0

+ Changed: Configuration API to have clearer syntax and behavior
+ Add: implicit cast sets; Guids and Enums are converted automatically
+ Fix: Weird formatting and new lines for nested tables
+ Fix: Invalid TOML strings produce better parser error message

## v0.4.2
 
+ Fix: Float was written as TomlInt when it had no decimal places [#8](https://github.com/paiden/Nett/issues/8)
+ Fix: Inline tables read as arrays [#7](https://github.com/paiden/Nett/issues/7)
+ Fix: Integer bare keys not working [#6](https://github.com/paiden/Nett/issues/6)

## v0.4.1

+ Add: Support for short date time formats
+ Fix: Writing files is culture invariant
+ Fix: Table encoding/decoding when they are used inside table arrays

## v0.4.0
+ First public preview release