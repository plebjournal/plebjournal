module PlebJournal.Dto.Models

open System
open Stacker.Calculate
open Stacker.Domain
open Stacker.GenerateSeries
//open Microsoft.AspNetCore.Http

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
type DcaCalculation =
    { StartDate: DateTime
      Amount: decimal
      Cadence: Cadence
      Duration: int
      DurationUnit: Duration }
    
[<CLIMutable>]
type UpdateSettings = { Fiat: Fiat }    
    
type TxHistoryViewModel =
    { Transaction: Transaction
      PercentChange: Change option
      Ngu: NgU option }
    
type FiatBalanceViewModel =
    { Balance: Balance
      CurrentValue: decimal<btc>
      Fiat: Fiat
      CostBasis: decimal
      Ngu: NgU option }
    
type CurrentBtcPrice =
    { Price: decimal
      Fiat: Fiat }
    
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


