module Stacker.Tests.Models

open System
open Stacker.Domain
open Stacker.Web.Models
open Stacker.Web.Models.Validation.Transaction
open Expecto
open Xunit

[<Fact>]
let ``should validate bad transaction`` () =
    let createTx = {
        Type = Buy
        BtcAmount = -0.5M
        BtcUnit = Sats
        FiatAmount = 0.0m
        Fiat = USD
        Date = DateTime(2023, 01, 01) 
    }
    
    let expected = Error [
        "Must be positive btc amount"
        "Must be positive Fiat amount"
    ]
    
    let actual = validateNewTransaction createTx
    Expect.equal actual expected "Should equal"