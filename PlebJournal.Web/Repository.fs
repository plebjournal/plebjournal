module Stacker.Web.Repository

open System
open System.Linq
open PlebJournal.Db
open Microsoft.EntityFrameworkCore
open PlebJournal.Db.Models
open Stacker
open Stacker.Charting.Domain
open Stacker.Domain

module WorkbenchChart =
    let mutable workbenchCharts: (string * GraphableDataSeries) list = []

module DcaCalculation =
    open GenerateSeries

    let mutable dcaCalculation =
        { Start = DateTime.Now.Date.AddYears(-4)
          Duration = 5, Years
          Cadence = Weekly
          FiatAmount = 500m }

module Transactions =
    let txToDao (user: PlebUser) (tx: Transaction) =
        match tx with
        | Buy { Id = id
                Date = dateTime
                Amount = btcAmount
                Fiat = fiatAmount } ->
            Transaction(
                Id = id,
                Date = dateTime,
                Type = tx.TxName,
                BtcAmount = decimal btcAmount.AsBtc,
                FiatAmount = fiatAmount.Amount,
                FiatCode = fiatAmount.Currency.ToString(),
                PlebUser = user
            )
        | Sell { Id = id
                 Date = dateTime
                 Amount = btcAmount
                 Fiat = fiatAmount } ->
            Transaction(
                Id = id,
                Date = dateTime,
                Type = tx.TxName,
                BtcAmount = (btcAmount.AsBtc |> decimal),
                FiatAmount = fiatAmount.Amount,
                FiatCode = fiatAmount.Currency.ToString(),
                PlebUser = user
            )
        | Income { Id = id
                   Date = dateTime
                   Amount = btcAmount } ->
            Transaction(
                Id = id,
                Date = dateTime,
                Type = tx.TxName,
                BtcAmount = (btcAmount.AsBtc |> decimal),
                PlebUser = user
            )
        | Spend { Id = id
                  Date = dateTime
                  Amount = btcAmount } ->
            Transaction(
                Id = id,
                Date = dateTime,
                Type = tx.TxName,
                BtcAmount = (btcAmount.AsBtc |> decimal),
                PlebUser = user
            )

    let txFromDao (dao: Models.Transaction) =
        let parseFiat (fiat: string) =
            match Fiat.fromString fiat with
            | Some f -> f
            | None -> failwith $"Unsupported fiat currency {fiat}"

        match dao.Type.ToLower() with
        | "buy" ->
            let fiat =
                { Amount = dao.FiatAmount.Value
                  Currency = parseFiat dao.FiatCode }

            Buy
                { Id = dao.Id
                  Date = dao.Date
                  Amount = Btc(someBtc dao.BtcAmount)
                  Fiat = fiat }
        | "sell" ->
            let fiat =
                { Amount = dao.FiatAmount.Value
                  Currency = parseFiat dao.FiatCode }

            Sell
                { Id = dao.Id
                  Date = dao.Date
                  Amount = Btc(someBtc dao.BtcAmount)
                  Fiat = fiat }
        | "income" ->
            Income
                { Id = dao.Id
                  Date = dao.Date
                  Amount = Btc(someBtc dao.BtcAmount) }
        | "spend" ->
            Spend
                { Id = dao.Id
                  Date = dao.Date
                  Amount = Btc(someBtc dao.BtcAmount) }
        | _ -> failwith $"Unsupported transaction type {dao.Type}"

    module Insert =
        let insertTx (db: PlebJournalDb) (tx: Transaction) (userId: Guid) =
            task {
                let! user = db.Users.FindAsync(userId)
                let txDao = txToDao user tx
                let! _ = db.Transactions.AddAsync(txDao)
                let! _ = db.SaveChangesAsync()
                return ()
            }

        let insertMany (db: PlebJournalDb) (txs: Transaction list) (userId: Guid) =
            task {
                let! user = db.Users.FindAsync(userId)
                let txDaos = txs |> List.map (txToDao user)
                let! _ = db.Transactions.AddRangeAsync(txDaos)
                let! _ = db.SaveChangesAsync()
                return ()
            }

    module Update =
        let updateTx (db: PlebJournalDb) (t: Transaction) (userId: Guid) =
            task {
                let! user = db.Users.FindAsync(userId)
                let dao = t |> txToDao user
                let! tx = db.Transactions.FindAsync(t.Id)

                if tx <> null then
                    tx.Date <- dao.Date.ToUniversalTime()
                    tx.Type <- dao.Type
                    tx.BtcAmount <- decimal dao.BtcAmount
                    tx.FiatAmount <- dao.FiatAmount
                    tx.FiatCode <- dao.FiatCode
                    tx.Updated <- DateTime.UtcNow
                    let! _ = db.SaveChangesAsync()
                    return Some()
                else
                    return None
            }

    module Read =
        let getAllTxsForUser (db: PlebJournalDb) (userId: Guid) =
            try
                task {
                    let! txs =
                        db.Transactions
                            .Where(fun t -> t.PlebUser.Id = userId)
                            .OrderByDescending(fun t -> t.Date)
                            .ToArrayAsync()

                    return txs |> Array.map txFromDao
                }
            with ex ->
                raise ex

        let getTxsForUserInHorizon (db: PlebJournalDb) (userId: Guid) (horizon: DateTime) =
            task {
                let! txsDao =
                    db.Transactions
                        .Where(fun t -> t.PlebUser.Id = userId && t.Date >= horizon)
                        .OrderByDescending(fun t -> t.Date)
                        .ToArrayAsync()

                return txsDao |> Array.map txFromDao
            }

        let getTxById (db: PlebJournalDb) (userId: Guid) (txId: Guid) =
            task {
                let! tx = db.Transactions.FirstOrDefaultAsync(fun t -> t.Id = txId && t.PlebUser.Id = userId)

                match tx with
                | null -> return None
                | _ -> return txFromDao tx |> Some
            }

    module Delete =
        let deleteTxForUser (db: PlebJournalDb) (txId: Guid) (userId: Guid) =
            task {
                let! tx = db.Transactions.FirstOrDefaultAsync(fun t -> t.Id = txId && t.PlebUser.Id = userId)
                let _ = db.Transactions.Remove(tx)
                let! _ = db.SaveChangesAsync()
                return ()
            }

module Prices =

    type PriceAtDateDao =
        { Id: Guid
          Price: decimal
          Date: DateTime
          Currency: string }

    module Read =
        open Stacker.Calculate

        let getMostRecentPrice (db: PlebJournalDb) (currency: Fiat) =
            task {
                let! res =
                    db.Prices
                        .Where(fun p -> p.Currency = currency.ToString())
                        .OrderByDescending(fun p -> p.Date)
                        .FirstOrDefaultAsync()

                return res |> Option.ofObj |> Option.map (fun p -> p.Date)
            }

        let getPrices (db: PlebJournalDb) (currency: Fiat) =
            task {
                let! prices =
                    db.Prices
                        .Where(fun p -> p.Currency = currency.ToString())
                        .OrderBy(fun p -> p.Date)
                        .ToArrayAsync()

                return prices |> Array.map (fun p -> { Price = p.BtcPrice; Date = p.Date })
            }
        
        let getPricesUntilDate (db: PlebJournalDb) (currency: Fiat) (date: DateTime) =
            task {
                return!
                    db.Prices
                        .Where(fun p -> p.Currency = currency.ToString() && p.Date >= date)
                        .OrderBy(fun p -> p.Date)
                        .ToArrayAsync()
            }

        let getPriceAtDate (db: PlebJournalDb) (currency: Fiat) (date: DateTime) =
            task {
                let! price =
                    db.Prices
                        .Where(fun p -> p.Currency = currency.ToString() && p.Date = date)
                        .FirstOrDefaultAsync()

                return price.BtcPrice
            }

    module Insert =
        let insertHistoricalPrices (db: PlebJournalDb) (prices: PriceAtDateDao list) =
            let toInsert =
                prices
                |> List.map (fun p ->
                    new Price(Date = p.Date.ToUniversalTime(), BtcPrice = p.Price, Currency = p.Currency))

            task {
                do! db.Prices.AddRangeAsync(toInsert)
                let! _ = db.SaveChangesAsync()
                return ()
            }

module CurrentPrice =

    module Update =
        let upsertCurrentPrice (db: PlebJournalDb) (price: Prices.PriceAtDateDao) =
            task {
                let! existing =
                    db.CurrentPrices
                        .Where(fun p -> p.Currency = price.Currency)
                        .FirstOrDefaultAsync()

                match (Option.ofObj existing) with
                | None ->
                    let toInsert =
                        CurrentPrice(
                            BtcPrice = price.Price,
                            Currency = price.Currency,
                            Created = DateTime.UtcNow,
                            Updated = DateTime.UtcNow
                        )

                    let! _ = db.AddAsync(toInsert)
                    let! _ = db.SaveChangesAsync()
                    return ()
                | Some currPrice ->
                    currPrice.BtcPrice <- price.Price
                    currPrice.Updated <- DateTime.UtcNow
                    let! _ = db.SaveChangesAsync()
                    return ()
            }

    module Read =
        let getCurrentPrice (db: PlebJournalDb) (fiat: Fiat) =
            task {
                let! p =
                    db.CurrentPrices
                        .Where(fun p -> p.Currency = fiat.ToString())
                        .FirstOrDefaultAsync()

                return p.BtcPrice
            }

module UserSettings =
    let private preferredFiat = "PreferredFiat"
    
    let getPreferredFiat (db: PlebJournalDb) (userId: Guid) = task {
        let! fiat =
            db.UserSettings
                .Where(fun us -> us.Name = preferredFiat && us.PlebUser.Id = userId)
                .FirstOrDefaultAsync()
        if fiat = null then return USD else
        return Fiat.fromString(fiat.Value) |> Option.defaultValue USD
    }
    
    let upsertSetting (db: PlebJournalDb) (userId: Guid) (name: string) (value: string) =
        task {
            let! user = db.Users.FindAsync(userId)
            let! existingSetting =
                db.UserSettings
                    .Where(fun us -> us.Name = preferredFiat && us.PlebUser.Id = userId)
                    .FirstOrDefaultAsync()
            
            if existingSetting = null then
                let setting = UserSetting(
                    Id = Guid.NewGuid(),
                    PlebUser = user,
                    Name = name,
                    Value = value
                )
                let! _ = db.UserSettings.AddAsync(setting)
                ()
            else
                existingSetting.Value <- value
            
            let! _ = db.SaveChangesAsync()
            return ()
        }
        
    let setPreferredFiat (db: PlebJournalDb) (userId: Guid) (fiat: Fiat) = task {
        do! upsertSetting db userId preferredFiat (fiat.ToString())
    }
    
module Notes =
    let fromDao (note: PlebJournal.Db.Models.Note) =
        let parseFiat currency =
            match Fiat.fromString currency with
            | Some f -> f
            | None -> failwith $"Unsupported fiat currency {currency}"
        
        { Id = note.Id
          Text = note.Text
          Sentiment = Sentiment.Parse note.Sentiment
          BtcPrice = note.Price
          Fiat = parseFiat note.Currency
          Date = note.Created.ToLocalTime() }
    
    let createNote (db: PlebJournalDb) (userId: Guid) (note: Note) =
        task {
            let! user = db.Users.FindAsync(userId)
            let newNote = PlebJournal.Db.Models.Note(
                PlebUser = user,
                Text = note.Text,
                Currency = note.Fiat.ToString(),
                Sentiment = (note.Sentiment |> Option.map string |> Option.defaultValue null),
                Price = note.BtcPrice,
                Created = note.Date
            )
            let! _ = db.Notes.AddAsync(newNote)
            let! _ = db.SaveChangesAsync()
            return ()
        }
    
    let getAll (db: PlebJournalDb) (userId: Guid) =
        task {
            let! notes =
                db.Notes
                    .Where(fun n -> n.PlebUser.Id = userId && n.Text.Length < 2048)
                    .OrderByDescending(fun n -> n.Created)
                    .ToArrayAsync()
            return notes |> Array.map fromDao 
        }
        
    let getNote (db: PlebJournalDb) (userId: Guid) (noteId: Guid) =
        task {
            let! note = db.Notes.FirstOrDefaultAsync(fun n -> n.Id = noteId && n.PlebUser.Id = userId)
            return if note = null then None else Some (fromDao note)
        }