module Stacker.Web.Import

open System
open FSharp.Data
open FsToolkit.ErrorHandling
open Stacker.Domain

type CsvImport =
    { Type: string
      Buy: decimal option
      BuyCurrency: string option
      Sell: decimal option
      SellCurrency: string option
      Date: DateTime }
    
let maybeDecimal (str: String) =
    match Decimal.TryParse(str) with
    | true, d -> Some d
    | false, _ -> None
    
let maybeCurrency (str: String) =
    match String.IsNullOrWhiteSpace(str) with
    | true -> None
    | false -> Some str

let readRow (row: CsvRow) =
    try
        { Type = row["Type"]
          Buy = row["Buy"] |> maybeDecimal
          BuyCurrency = row["BuyCurrency"] |> maybeCurrency
          Sell = row["Sell"] |> maybeDecimal
          SellCurrency = row["SellCurrency"] |> maybeCurrency
          Date = row["Date"].AsDateTime() } |> Ok
    with
    | ex ->
        Error ex.Message
    
module Parse =
    let notNullOrEmpty (msg: string) (s: string) =
        if String.IsNullOrWhiteSpace(s) then Error $"{msg}" else Ok s
        
    let requiredField (opt: 'a option) =
        match opt with
        | Some opt -> Ok opt
        | None -> Error ("Missing required field")
        
    let parseFiat (fiatStr: string) =
        match fiatStr.ToLower() with
        | "usd" -> Ok USD
        | "eur" -> Ok EUR
        | "cad" -> Ok CAD
        | _ -> Error $"Unsupported currency: {fiatStr}"
    
    let parseTrade (csvImport: CsvImport) =
        validation {
            let! buyCurrency = csvImport.BuyCurrency |> requiredField |> Result.map (fun s -> s.ToLower())
            let! sellCurrency = csvImport.SellCurrency |> requiredField |> Result.map (fun s -> s.ToLower())
            let! buyAmount = csvImport.Buy |> requiredField
            let! sellAmount = csvImport.Sell |> requiredField
            if buyCurrency = "btc" then
                let! fiat = parseFiat sellCurrency
                let fiatAmount = { Amount = sellAmount; Currency = fiat }
                let btcAmount = Btc(buyAmount * 1.0m<btc>)
                return Buy { Id = Guid.NewGuid(); Date = csvImport.Date; Amount =  btcAmount; Fiat = fiatAmount }
            else if sellCurrency = "btc" then
                let! fiat = parseFiat buyCurrency
                let fiatAmount = { Amount = buyAmount; Currency = fiat }
                let btcAmount = Btc(sellAmount * 1.0m<btc>)
                return Sell { Id = Guid.NewGuid(); Date = csvImport.Date; Amount =  btcAmount; Fiat = fiatAmount }
            else
                return! Error $"Unsupported Trade Pair {buyCurrency} - {sellCurrency}"
         }
        
    let parseIncome (csvImport: CsvImport) =
        validation {
            let! incomeAmount = csvImport.Buy |> requiredField |> Result.map (fun d -> d * 1.0m<btc>)
            return Income { Id = Guid.NewGuid(); Date = csvImport.Date; Amount =  Btc(incomeAmount) }
        }
        
    let parseMining (csvImport: CsvImport) =
        let mustBeBtc (str) = if str = "btc" then Ok str else Error $"Unsupported mining currency {str}" 
        validation {
            let! currency = csvImport.BuyCurrency |> requiredField |> Result.map (fun s -> s.ToLower())
            let! btc = mustBeBtc currency
            let! miningAmount = csvImport.Buy |> requiredField |> Result.map (fun d -> d * 1.0m<btc>)
            return Income { Id = Guid.NewGuid(); Date = csvImport.Date; Amount = Btc(miningAmount)}
        }
    
let toDomain (csvImport: CsvImport) =
    validation {
        let! rowType = csvImport.Type |> Parse.notNullOrEmpty "Record Type Required" |> Result.map (fun s -> s.ToLower())
        let! row =
            if rowType = "trade" then Parse.parseTrade csvImport
            else if rowType = "income" then Parse.parseIncome csvImport
            else if rowType = "mining" then Parse.parseMining csvImport
            else
                Error [ $"Unsupported Tx type: {rowType}" ]
        return row
    }

module List =
    let oks (lst: Result<'a, 'b> list) =
        lst
        |> List.collect (fun r ->
            match r with
            | Ok res -> [res]
            | Error _ -> [])

let import (csv: string) =
    let file = CsvFile.Parse(csv)
    file.Rows
    |> Seq.toList
    |> List.map readRow
    |> List.oks
    |> List.map toDomain    