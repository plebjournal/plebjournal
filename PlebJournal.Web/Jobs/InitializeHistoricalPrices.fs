module Stacker.Web.Jobs.InitializePrices

open System.IO
open System.Text.Json
open Microsoft.Extensions.Logging
open Quartz
open Stacker.ExtendedTypes
open Stacker.Domain
open Stacker.Web
open Stacker.Web.Repository.PostgresDb.Prices

let updateHistorical (logger: ILogger) (currency: Fiat) =
    task {
        logger.LogInformation("Starting updating historical prices for {currency}", currency.ToString())
        let! prices = CoinGecko.fetchHistoricalMarket currency
        let! mostRecent = Read.getMostRecentPrice currency

        match mostRecent with
        | None ->
            logger.LogInformation("Adding {count} prices for {currency}", prices.Length, currency.ToString())
            let toInsert = List.exceptLast prices
            do! Insert.insertHistoricalPrices toInsert
        | Some d ->
            let updates = prices |> List.where (fun p -> p.Date > d) |> List.exceptLast

            logger.LogInformation("Adding {count} prices for {currency}", updates.Length, currency.ToString())
            do! Insert.insertHistoricalPrices updates

        logger.LogInformation("Finished updating historical prices for {currency}", currency.ToString())
    }

let initializeHistoricalUsd (logger: ILogger) =
    logger.LogInformation("Reading historical BTC USD prices from json")
    let raw = File.ReadAllText("./Data/historical-usd-prices.json")

    let config = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let prices = JsonSerializer.Deserialize<PriceAtDateDao list>(raw, config)
    logger.LogInformation("Done reading historical BTC USD prices")

    task {
        for price in prices do
            do! Insert.insertUnique price

        logger.LogInformation("Finished inserting historical btc usd prices to db")
    }

/// Seeds the prices table with historical USD.
/// USD prices go back to 2010.
/// Use Coin Gecko's API to fetch more recent prices
/// Other currencies are seeded, but don't go back as far as USD.
type InitHistorical(logFactory: ILoggerFactory) =
    interface IJob with
        member this.Execute _ =
            task { do! initializeHistoricalUsd (logFactory.CreateLogger("Historical BTC USD")) }

/// Periodically fill in any missing price data
/// Run once a day
type UpdateHistorical(logFactory: ILoggerFactory) =
    interface IJob with
        member this.Execute _ =
            task {
                let logger = logFactory.CreateLogger("Update Historical")
                logger.LogInformation("Updating historical prices")
                do! updateHistorical logger USD
                do! updateHistorical logger CAD
                do! updateHistorical logger EUR
            }
