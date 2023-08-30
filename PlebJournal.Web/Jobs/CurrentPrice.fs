module Stacker.Web.Jobs.CurrentPrice

open Microsoft.Extensions.Logging
open PlebJournal.Db
open Quartz
open Stacker.Web.CoinGecko
open Stacker.Domain
open Stacker.Web.Repository.CurrentPrice.Update

type CurrentPrice(loggerFactory: ILoggerFactory, db: PlebJournalDb) =
    interface IJob with
        member this.Execute _ =
            let logger = loggerFactory.CreateLogger("Current Price")
            logger.LogInformation("Updating current fiat prices")

            task {
                let! cad = fetchCurrentPrice CAD
                let! eur = fetchCurrentPrice EUR
                let! usd = fetchCurrentPrice USD

                logger.LogInformation("Loaded price {price} {currency} at {date}", usd.Price, usd.Currency, usd.Date)
                do! upsertCurrentPrice db usd

                logger.LogInformation("Loaded price {price} {currency} at {date}", cad.Price, cad.Currency, cad.Date)
                do! upsertCurrentPrice db cad

                logger.LogInformation("Loaded price {price} {currency} at {date}", eur.Price, eur.Currency, eur.Date)
                do! upsertCurrentPrice db eur

                return ()
            }
