module Stacker.Web.CoinGecko

open System
open System.Net
open Stacker.Domain
open Repository.Prices
open System.Net.Http
open FsHttp

let coinGeckoBaseUrl = "https://api.coingecko.com/api/v3"

type BtcMarketResponse = { Prices: float list list }

type BtcSimplePriceResponse =
    { Bitcoin: {| Usd: float; Cad: float; Eur: float |} }

let marketResponseToPriceDao (fiat: Fiat) (coinGecko: BtcMarketResponse) =
    coinGecko.Prices
    |> List.map (fun dateAndPrice ->
        match dateAndPrice with
        | [ timestamp; price ] ->
            { Id = Guid.NewGuid()
              Price = decimal price
              Date = int64 timestamp |> DateTimeOffset.FromUnixTimeMilliseconds |> fun d -> d.UtcDateTime.Date
              Currency = fiat.ToString() }
        | _ -> failwith "Failed to read response from coingecko")

let simpleResponseToPriceDao (fiat: Fiat) (coinGecko: BtcSimplePriceResponse) =
    let price =
        match fiat with
        | USD -> coinGecko.Bitcoin.Usd
        | CAD -> coinGecko.Bitcoin.Cad
        | EUR -> coinGecko.Bitcoin.Eur

    { Id = Guid.NewGuid()
      Price = decimal price
      Date = DateTime.Now
      Currency = fiat.ToString() }

let fetchHistoricalMarket (currency: Fiat) =
    let symbol = currency.ToString().ToLower()

    task {
        let! resp =
            http {
                GET $"{coinGeckoBaseUrl}/coins/bitcoin/market_chart"
                query [ "vs_currency", symbol; "days", "max" ]
            }
            |> Request.sendAsync

        return
            match resp.statusCode with
            | HttpStatusCode.OK ->
                Json.deserialize<BtcMarketResponse> (resp.content.ReadAsStream())
                |> marketResponseToPriceDao currency
            | _ -> failwith $"Could not fetch prices from {coinGeckoBaseUrl}"
    }

let fetchCurrentPrice (currency: Fiat) =
    let symbol = currency.ToString().ToLower()

    task {
        let! resp =
            http {
                GET $"{coinGeckoBaseUrl}/simple/price"
                query [ "ids", "bitcoin"; "vs_currencies", symbol ]
            }
            |> Request.sendAsync

        return
            match resp.statusCode with
            | HttpStatusCode.OK ->
                Json.deserialize<BtcSimplePriceResponse> (resp.content.ReadAsStream())
                |> simpleResponseToPriceDao currency
            | _ -> failwith $"Could not fetch price for BTC/{symbol} from coingecko"
    }
