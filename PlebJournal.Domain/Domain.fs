﻿namespace Stacker

open System
open Microsoft.FSharp.Reflection

module Domain =
    [<Measure>]
    type btc
    let someBtc (d: decimal) = d * 1.0m<btc>

    [<Measure>]
    type sats

    let satsPerBtc: decimal<sats / btc> = 100000000m<sats / btc>

    let btcPerSats: decimal<btc / sats> = 0.00000001m<btc / sats>

    let convertSatsToBtc (sats: decimal<sats>) = sats * btcPerSats
    let convertBtcToSats (btc: decimal<btc>) = btc * satsPerBtc

    let convertSatsToBtcInt64 (sats: int64<sats>) =
        (decimal sats) * (1.0m<sats>) |> convertSatsToBtc

    let convertBtcToSatsDecimal (btc: decimal<btc>) =
        convertBtcToSats btc |> decimal |> Decimal.ToInt64 |> (*) 1L<sats>

    type BtcAmount =
        | Btc of decimal<btc>
        | Sats of int64<sats>
        static member OfBtc (d: decimal) =
            Btc(d * 1.0m<btc>)
            
        static member OfSats (sats: int64) =
            Sats(sats * 1L<sats>)

        member this.AsBtc =
            match this with
            | Btc d -> d
            | Sats s -> convertSatsToBtcInt64 s

        member this.AsSats =
            match this with
            | Btc btc -> convertBtcToSatsDecimal btc
            | Sats sats -> sats

    type Fiat =
        | CAD
        | USD
        | EUR
        override this.ToString() =
            match this with
            | CAD -> "CAD"
            | USD -> "USD"
            | EUR -> "EUR"
            
        static member fromString(str: string) =
            match str.ToUpperInvariant() with
            | "USD" -> Some USD
            | "CAD" -> Some CAD
            | "EUR" -> Some EUR
            | _ -> None

    type Sentiment =
        | Resilient
        | Patient
        | Bored
        | Hopeful
        | Optimistic
        | FOMO
        | Euphoric
        | Skeptical
        | Cautious
        | Regretful
        | Anxious
        | Panic
        | Exhausted
        
        static member AllCases =
            FSharpType.GetUnionCases(typeof<Sentiment>)
            |> Array.map (fun a -> a.Name)
            |> Array.toList
        
        static member Parse(str: string) =
            match str with
            | null -> None
            | "" -> None
            | "Resilient" -> Some Resilient
            | "Patient" -> Some Patient
            | "Bored" -> Some Bored
            | "Hopeful" -> Some Hopeful
            | "Optimistic" ->  Some Optimistic
            | "FOMO" ->  Some FOMO
            | "Euphoric" -> Some Euphoric
            | "Skeptical" -> Some Skeptical
            | "Cautious" -> Some Cautious
            | "Regretful" -> Some Regretful
            | "Anxious" -> Some Anxious
            | "Panic" -> Some Panic
            | "Exhausted" -> Some Exhausted
            | _ -> None
    
    type Note =
        { Id: Guid
          Text: string
          Sentiment: Sentiment option
          BtcPrice: decimal
          Fiat: Fiat
          Date: DateTime }    
    
    type FiatAmount =
        { Amount: decimal
          Currency: Fiat }
       
    type Income =
        { Id: Guid
          Date: DateTime
          Amount: BtcAmount }
    
    type Spend =
        { Id: Guid
          Date: DateTime
          Amount: BtcAmount }
    type Buy =
        { Id: Guid
          Date: DateTime
          Amount: BtcAmount
          Fiat: FiatAmount }
        
    type Sell =
        { Id: Guid
          Date: DateTime
          Amount: BtcAmount
          Fiat: FiatAmount }

    type Transaction =
        | Income of Income
        | Spend of Spend
        | Buy of Buy
        | Sell of Sell
        
        member this.Id =
            match this with
            | Spend { Id = id } -> id
            | Income { Id = id } -> id
            | Buy { Id = id } -> id
            | Sell { Id = id } -> id
        
        member this.DateTime =
            match this with
            | Spend s -> s.Date
            | Income i -> i.Date
            | Buy b -> b.Date
            | Sell s -> s.Date

        member this.Amount =
            match this with
            | Spend s -> s.Amount.AsBtc
            | Income i -> i.Amount.AsBtc
            | Buy b -> b.Amount.AsBtc
            | Sell s -> s.Amount.AsBtc

        member this.Fiat =
            match this with
            | Spend _ -> None
            | Income _ -> None
            | Buy b -> Some b.Fiat
            | Sell s -> Some s.Fiat

        member this.TxName =
            match this with
            | Spend _ -> "SPEND"
            | Buy _ -> "BUY"
            | Sell _ -> "SELL"
            | Income _ -> "INCOME"

        member this.PricePerCoin =
            let btcAmount = this.Amount
            let fiat = this.Fiat
            
            if btcAmount = 0.0m<btc> then None else

            match fiat with
            | Some a ->
                let perCoin = a.Amount / btcAmount |> decimal
                (perCoin, a.Currency) |> Some
            | None -> None
   