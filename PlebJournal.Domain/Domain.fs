namespace Stacker

open System

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

    let convertSatsToBtcInt (sats: int<sats>) =
        (decimal sats) * (1.0m<sats>) |> convertSatsToBtc

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
        member this.ToString() =
            match this with
            | CAD -> "CAD"
            | USD -> "USD"
            | EUR -> "EUR"

    type FiatAmount =
        { Amount: decimal
          Currency: Fiat }

    type Transaction =
        | Income of DateTime * BtcAmount
        | Spend of DateTime * BtcAmount
        | Buy of DateTime * BtcAmount * FiatAmount
        | Sell of DateTime * BtcAmount * FiatAmount

        member this.DateTime =
            match this with
            | Spend (d, _) -> d
            | Income (d, _) -> d
            | Buy (d, _, _) -> d
            | Sell (d, _, _) -> d

        member this.Amount =
            match this with
            | Spend (_, amt) -> amt.AsBtc
            | Income (_, amt) -> amt.AsBtc
            | Buy (_, amt, _) -> amt.AsBtc
            | Sell (_, amt, _) -> amt.AsBtc

        member this.Fiat =
            match this with
            | Spend _ -> None
            | Income _ -> None
            | Buy (_, _, fiat) -> Some fiat
            | Sell (_, _, fiat) -> Some fiat

        member this.TxName =
            match this with
            | Spend _ -> "SPEND"
            | Buy _ -> "BUY"
            | Sell _ -> "SELL"
            | Income _ -> "INCOME"

        member this.PricePerCoin =
            let btcAmount = this.Amount
            let fiat = this.Fiat

            match fiat with
            | Some a ->
                let fiatAmount = a.Amount
                let perCoin = fiatAmount / btcAmount |> decimal
                (perCoin, a.Currency) |> Some
            | None -> None
