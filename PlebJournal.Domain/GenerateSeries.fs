namespace Stacker

open System
open Stacker.Domain
open Stacker.Calculate
open Stacker.ExtendedTypes
open NCrontab

module GenerateSeries =

    type Cadence =
        | Daily
        | Weekly
        | Monthly

    type Duration =
        | Days
        | Weeks
        | Months
        | Years

    type GenerateRequest =
        { Start: DateTime
          Duration: int * Duration
          Cadence: Cadence
          FiatAmount: decimal }

        member this.EndDate =
            match this.Duration with
            | i, Days -> this.Start.AddDays(i)
            | i, Weeks -> this.Start.AddDays(float i * 7.0)
            | i, Months -> this.Start.AddMonths(i)
            | i, Years -> this.Start.AddYears(i)

        member this.Cron =
            match this.Cadence with
            | Daily -> "0 0 * * *"
            | Weekly -> "0 0 * * 0"
            | Monthly -> "0 0 1 * *"

    let generateDates (req: GenerateRequest) =
        let start = req.Start
        let endDate = req.EndDate
        let schedule = CrontabSchedule.Parse req.Cron
        start :: (schedule.GetNextOccurrences(start, endDate) |> Seq.toList)

    let private generatePurchasesFromDate
        (allPrices: PriceAtDate list)
        (fiatAmountUsd: decimal)
        (dates: DateTime list)
        =
        let pricesAsMap = allPrices |> List.map (fun p -> p.Date, p.Price) |> Map.ofList

        dates
        |> List.map (fun d ->
            let priceToUse = pricesAsMap |> Map.tryFind d

            match priceToUse with
            | Some price ->
                let btcAmount = fiatAmountUsd / price |> fun btc -> Btc(btc * 1.0m<btc>)

                Buy
                    { Id = Guid.Empty
                      Date = d
                      Amount = btcAmount
                      Fiat = { Currency = USD; Amount = fiatAmountUsd } }
                |> Some
            | None -> None)
        |> List.somes

    let generate (priceHistory: PriceAtDate list) (pattern: GenerateRequest) =
        generateDates pattern
        |> generatePurchasesFromDate priceHistory pattern.FiatAmount
