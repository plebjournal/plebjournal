module Stacker.Web.Models

open System
open Stacker.Domain
open Stacker.GenerateSeries
open Microsoft.AspNetCore.Http

type TxType = Buy | Sell | Income | Spend

type BtcUnit = Btc | Sats

[<CLIMutable>]
type CreateBtcTransaction =
    { Type: TxType
      BtcAmount: decimal
      BtcUnit: BtcUnit
      FiatAmount: decimal
      Date: DateTime
      TimeZoneOffset: int
      Fiat: Fiat }

[<CLIMutable>]
type EditBtcTransaction =
    { Id: Guid
      Type: TxType
      Amount: decimal
      BtcUnit: BtcUnit
      FiatAmount: decimal
      Date: DateTime
      TimeZoneOffset: int
      AmountType: string
      Fiat: Fiat }

type Balance = { Total: decimal<btc> }

[<CLIMutable>]
type CreateNewAccount =
    { Username: string
      Password: string
      PasswordRepeat: string
      Fiat: Fiat }
    
type CreateAccountErrors =
    { Username: string option
      Password: string option
      Identity: string list }

[<CLIMutable>]
type Login =
    { Username: string
      Password: string }
    
[<CLIMutable>]   
type Formula =
    { Formula: string
      FormulaName: string }
    
[<CLIMutable>]
type Import =
    { CsvFile: IFormFile
      Other: string }    

[<CLIMutable>]
type DcaCalculation =
    { StartDate: DateTime
      Amount: decimal
      Cadence: Cadence
      Duration: int
      DurationUnit: Duration }
    
type TxHistoryHorizon =
    | TwoMonths
    | TwelveMonths
    | TwoYears
    | AllData
    static member parse (horizon: string) =
        match horizon with
        | "2-months" -> TwoMonths |> Some
        | "12-months" -> TwelveMonths |> Some
        | "24-months" -> TwoYears |> Some
        | "all-data" -> AllData |> Some
        | _ -> None
        
module Validation =
    open FsToolkit.ErrorHandling
    
    let validateNewTransaction (tx: CreateBtcTransaction) =
        let validateBtcAmount (tx: CreateBtcTransaction) =
            if tx.BtcAmount <= 0.0m then Error "Must be positive btc amount" else
                let asSats =
                    match tx.BtcUnit with
                    | Btc -> convertBtcToSatsDecimal (tx.BtcAmount * 1.0M<btc>)
                    | Sats -> int64 tx.BtcAmount * 1L<sats>
            
                if asSats < 1L<sats> then Error "Must be a positive amount of btc" else Ok asSats
        let validateFiatAmount (tx: CreateBtcTransaction) =
            if tx.FiatAmount <= 0.0m then Error "Must be positive Fiat amount" else Ok tx.FiatAmount
        let validateDate (tx: CreateBtcTransaction) =
            match tx.Date with
            | d when d < DateTime(2009, 01, 01) -> Error "Bitcoin wasn't invented yet!"
            | _ -> Ok tx.Date
        validation {
            let! btcAmount = validateBtcAmount tx
            and! fiatAmount = validateFiatAmount tx
            and! date = validateDate tx
            
            return tx
        }
        
    let validateEditedTransaction (tx: EditBtcTransaction) =
        let validateBtcAmount (tx: EditBtcTransaction) =
            if tx.Amount <= 0.0m then Error "Must be positive btc amount" else
                let asSats =
                    match tx.BtcUnit with
                    | Btc -> convertBtcToSatsDecimal (tx.Amount * 1.0M<btc>)
                    | Sats -> int64 tx.Amount * 1L<sats>
            
                if asSats < 1L<sats> then Error "Must be a positive amount of btc" else Ok asSats
        let validateFiatAmount (tx: EditBtcTransaction) =
            if tx.FiatAmount <= 0.0m then Error "Must be positive Fiat amount" else Ok tx.FiatAmount
        let validateDate (tx: EditBtcTransaction) =
            match tx.Date with
            | d when d < DateTime(2009, 01, 01) -> Error "Bitcoin wasn't invented yet!"
            | _ -> Ok tx.Date
        validation {
            let! btcAmount = validateBtcAmount tx
            and! fiatAmount = validateFiatAmount tx
            and! date = validateDate tx
            
            return tx
        }        