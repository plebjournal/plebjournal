module PlebJournal.Dto.Validation

open System
open Stacker.Domain
open PlebJournal.Dto.Models
open FsToolkit.ErrorHandling

module Transaction =
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
        
module CreateAccount =
    let validateCreateAccount (create: CreateNewAccount) =
        if create.Password <> create.PasswordRepeat then
            Error { Username = None; Password = Some "Passwords should match"; Identity = [] }
        else if String.IsNullOrWhiteSpace(create.Username) then
            Error { Username = Some "Username cannot be empty"; Password = None; Identity = [] } else
        Ok create