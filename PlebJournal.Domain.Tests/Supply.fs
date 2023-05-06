module Stacker.Tests.Supply

open Stacker.Domain
open Stacker.Supply
open Xunit

open FsUnit.Xunit

[<Fact>]
let ``should calculate supply``() =
    btcSupply 0
    |> should equal 5_000_000_000L<sats>
    
    btcSupply 1
    |> should equal 10_000_000_000L<sats>
    
    btcSupply 209_999
    |> should equal (5_000_000_000L<sats> * 210_000L)
    
    btcSupply 210_000
    |> should equal
           ((5_000_000_000L<sats> * 210_000L) + 2_500_000_000L<sats>)

[<Fact>]
let ``should calculate max supply`` () =
    btcSupply 6_929_998
    |> should equal 2_099_999_997_689_999L
    
    // last block - max supply reached
    btcSupply 6_929_999
    |> should equal 2_099_999_997_690_000L    
    
    btcSupply 6_930_000
    |> should equal 2_099_999_997_690_000L
    
    btcSupply 7_000_000
    |> should equal 2_099_999_997_690_000L

[<Fact>]
let ``should handle bad inputs`` () =
    shouldFail (fun () -> btcSupply -1 |> ignore)
    
    shouldFail (fun () -> (btcSupply -10) |> ignore)