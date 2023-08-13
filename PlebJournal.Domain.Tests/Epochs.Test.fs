module Stacker.Tests.Epochs

open Stacker.Domain
open Stacker.Supply
open Stacker.Epochs
open Xunit

open FsUnit.Xunit

[<Fact>]
let ``should calculate epochs with block height`` () =
    let res = epochs 810_000
    res |> should haveLength 33