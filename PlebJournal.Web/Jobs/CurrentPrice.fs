module Stacker.Web.Jobs.CurrentPrice

open Microsoft.Extensions.Logging
open Quartz
open Stacker.Web.CoinGecko
open Stacker.Domain
open Stacker.Web.Repository.PostgresDb.CurrentPrice.Update

type CurrentPrice(loggerFactory: ILoggerFactory) =
    interface IJob with
        member this.Execute _ =
            let logger = loggerFactory.CreateLogger("Current Price")
            logger.LogInformation("Updating current fiat prices")
            
            task {
                // run these tasks in parallel
                let cad' = fetchCurrentPrice CAD
                let eur' = fetchCurrentPrice EUR
                let usd' = fetchCurrentPrice USD
                
                let! usd = usd'
                logger.LogInformation("Loaded price {price} {currency} at {date}", usd.Price, usd.Currency, usd.Date)
                do! upsertCurrentPrice usd
                
                let! cad = cad'
                logger.LogInformation("Loaded price {price} {currency} at {date}", cad.Price, cad.Currency, cad.Date)
                do! upsertCurrentPrice cad
                
                let! eur = eur'
                logger.LogInformation("Loaded price {price} {currency} at {date}", eur.Price, eur.Currency, eur.Date)
                do! upsertCurrentPrice eur
                
                return ()
            }
            
           