
module ``Read TOML from file``

open System
open Nett
open FluentAssertions
open Xunit

[<Fact>]
let ``With special characters in file, reads content correct``() =
    // Act
    let tml = Toml.ReadFile("TomlWithSpecialCharacters.tml");

    // Assert
    tml.Get<string>("p0").Should().Be(@"c:\äöü", null) |> ignore
