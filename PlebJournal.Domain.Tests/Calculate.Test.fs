module Stacker.Domain.Tests.Calculate

open System
open Xunit
open Expecto
open FsUnit.Xunit
open Stacker.Domain
open Stacker.Calculate

module Fold =
    [<Fact>]
    let ``should sum transactions`` () =
        [
            Income { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount =  BtcAmount.OfBtc 0.5m }
            Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }
            Spend { Id = Guid.Empty; Date = DateTime(2023, 01, 02); Amount =  BtcAmount.OfBtc 0.75m }
        ]
        |> foldTxs
        |> should equal 0.750m<btc>

module FoldDailyTransactions =
    [<Fact>]
    let ``should fold transactions on the same day``() =
        [
            Income { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount =  BtcAmount.OfBtc 0.5m }
            Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }
            Spend { Id = Guid.Empty; Date = DateTime(2023, 01, 02); Amount =  BtcAmount.OfBtc 0.75m }
        ]
        |> foldDailyTransactions
        |> Seq.toList
        |> should equal [
            DateTime(2023, 01, 01), someBtc 1.5m
            DateTime(2023, 01, 02), someBtc -0.75m
        ]
        
module MovingAverage =
    [<Fact>]
    let ``should calculate simple moving average`` () =
        let prices = [|
            { PriceAtDate.Date = DateTime(2023, 01, 01); Price = 200m }
            { PriceAtDate.Date = DateTime(2023, 01, 02); Price = 200m }
            { PriceAtDate.Date = DateTime(2023, 01, 03); Price = 200m }
            { PriceAtDate.Date = DateTime(2023, 01, 04); Price = 400m }
            { PriceAtDate.Date = DateTime(2023, 01, 05); Price = 500m }
        |]
        let expected = [
            { Date = DateTime(2023, 01, 03)
              Price = 200m
              MA = 200m }
            { Date = DateTime(2023, 01, 04)
              Price = 400m
              MA = 266.67m }
            { Date = DateTime(2023, 01, 05)
              Price = 500m
              MA = 366.67m }
        ]
        
        let actual = ma prices 3 |> Seq.sortBy (fun p -> p.Date)
        
        Expect.sequenceEqual actual expected "should equal"
        
module PortfolioHistoricalValue =
    [<Fact>]
    let ``should generate a series of dates and value for a portfolio`` () =
        let txs = [
            Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }
            Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 05); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1250m; Currency = USD } }

        ]
        
        let usdPriceHistory = [|
            { PriceAtDate.Date = DateTime(2023, 01, 01); Price = 1000m }
            { PriceAtDate.Date = DateTime(2023, 01, 02); Price = 1050m }
            { PriceAtDate.Date = DateTime(2023, 01, 03); Price = 1100m }
            { PriceAtDate.Date = DateTime(2023, 01, 04); Price = 1150m }
            { PriceAtDate.Date = DateTime(2023, 01, 05); Price = 1200m }
            { PriceAtDate.Date = DateTime(2023, 01, 06); Price = 1250m }
        |]
        
        let res = portfolioHistoricalValue txs usdPriceHistory
        Expect.sequenceEqual res [
            usdPriceHistory[0], 1.00m<btc>, 1000.00m
            usdPriceHistory[1], 1.00m<btc>, 1050.00m
            usdPriceHistory[2], 1.00m<btc>, 1100.00m
            usdPriceHistory[3], 1.00m<btc>, 1150.00m
            usdPriceHistory[4], 2.00m<btc>, 2400.00m
            usdPriceHistory[5], 2.00m<btc>, 2500.00m
        ] "should equal"
        
    [<Fact>]
    let ``should handle empty portfolio`` () =
        let usdPriceHistory = [|
            { PriceAtDate.Date = DateTime(2023, 01, 01); Price = 1000m }
            { PriceAtDate.Date = DateTime(2023, 01, 02); Price = 1050m }
            { PriceAtDate.Date = DateTime(2023, 01, 03); Price = 1100m }
            { PriceAtDate.Date = DateTime(2023, 01, 04); Price = 1150m }
            { PriceAtDate.Date = DateTime(2023, 01, 05); Price = 1200m }
            { PriceAtDate.Date = DateTime(2023, 01, 06); Price = 1250m }
        |]
        
        let res = portfolioHistoricalValue [] usdPriceHistory
        Expect.sequenceEqual res [ ] "should equal"        

module PercentChange =
    [<Fact>]
    let ``should calculate percent increase``() =
        Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }
        |> percentChange 2000m
        |> should equal (Some (Increase 100.0m))
    
    [<Fact>]
    let ``should calculate small percent increase``() =
        Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }
        |> percentChange 1020m
        |> should equal (Some (Increase 2.0m))    
    
    [<Fact>]
    let ``should calculate percent decrease`` () =
        Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }        
        |> percentChange 500m
        |> should equal (Some (Decrease 50m))
        
    [<Fact>]
    let ``should handle no change in price`` () =
        Buy { Id = Guid.Empty; Date = DateTime(2023, 01, 01); Amount = BtcAmount.OfBtc(1.0m); Fiat = { Amount = 1000m; Currency = USD } }        
        |> percentChange 1000m
        |> should equal (Some (Increase 0m))
