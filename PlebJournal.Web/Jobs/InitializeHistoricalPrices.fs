module Stacker.Web.Jobs.InitializePrices

open Microsoft.Extensions.Logging
open PlebJournal.Db
open Quartz
open Stacker.ExtendedTypes
open Stacker.Domain
open Stacker.Web
open Stacker.Web.Repository.Prices

let updateHistorical (db: PlebJournalDb) (logger: ILogger) (currency: Fiat) =
    task {
        logger.LogInformation("Starting updating historical prices for {currency}", currency.ToString())
        let! prices = CoinGecko.fetchHistoricalMarket currency
        let! mostRecent = Read.getMostRecentPrice db currency

        match mostRecent with
        | None ->
            logger.LogInformation("Adding {count} prices for {currency}", prices.Length, currency.ToString())
            let toInsert = List.exceptLast prices
            do! Insert.insertHistoricalPrices db toInsert
        | Some d ->
            let updates = prices |> List.where (fun p -> p.Date > d) |> List.exceptLast

            logger.LogInformation("Adding {count} prices for {currency}", updates.Length, currency.ToString())
            do! Insert.insertHistoricalPrices db updates

        logger.LogInformation("Finished updating historical prices for {currency}", currency.ToString())
    }

/// Periodically fill in any missing price data
/// Run once a day
type UpdateHistorical(logFactory: ILoggerFactory, db: PlebJournalDb) =
    interface IJob with
        member this.Execute _ =
            task {
                let logger = logFactory.CreateLogger("Update Historical")
                logger.LogInformation("Updating historical prices")
                do! updateHistorical db logger USD
                do! updateHistorical db logger CAD
                do! updateHistorical db logger EUR
            }
