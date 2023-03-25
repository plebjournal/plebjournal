module Stacker.Domain.Tests.GenerateSeries

open System
open Xunit
open FsUnit.Xunit
open Stacker.GenerateSeries

[<Fact>]
let ``should generate simple daily series``() =
    let pattern =
        { Cadence = Daily
          Start = DateTime(2023, 01, 14)
          Duration = 7, Days
          FiatAmount = 100.0m }
    pattern
    |> generateDates
    |> should equal [
        DateTime(2023, 01, 14)
        DateTime(2023, 01, 15)
        DateTime(2023, 01, 16)
        DateTime(2023, 01, 17)
        DateTime(2023, 01, 18)
        DateTime(2023, 01, 19)
        DateTime(2023, 01, 20)
    ]

[<Fact>]
let ``should generate weekly series``() =
    let pattern =
        { Cadence = Weekly
          Start = DateTime(2023, 01, 01)
          Duration = 3, Weeks
          FiatAmount = 0m }
    pattern
    |> generateDates
    |> should equal [
        DateTime(2023, 01, 01)
        DateTime(2023, 01, 08)
        DateTime(2023, 01, 15)
    ]
    
[<Fact>]
let ``should generate monthly series``() =
    let pattern =
        { Cadence = Monthly
          Start = DateTime(2023, 01, 01)
          Duration = 3, Months
          FiatAmount = 0m }
    pattern
    |> generateDates
    |> should equal [
        DateTime(2023, 01, 01)
        DateTime(2023, 02, 01)
        DateTime(2023, 03, 01)
    ]
   
    
    
[<Fact>]
let ``use cron library``() =
    let pattern =
        { Cadence = Daily
          Start = DateTime(2023, 01, 01)
          Duration = 1, Weeks
          FiatAmount = 0m }
    let res = generateDates pattern
    res
    |> should equal [
        DateTime(2023, 01, 01)
        DateTime(2023, 01, 02)
        DateTime(2023, 01, 03)
        DateTime(2023, 01, 04)
        DateTime(2023, 01, 05)
        DateTime(2023, 01, 06)
        DateTime(2023, 01, 07)
    ]
    