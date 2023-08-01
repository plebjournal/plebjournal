module Stacker.Web.Repository

open System
open PlebJournal.Db
open Microsoft.EntityFrameworkCore
open PlebJournal.Db.Models
open Stacker
open Stacker.Charting.Domain
open Stacker.Domain
open Stacker.Web.Db
            
module WorkbenchChart =
    let mutable workbenchCharts : (string * GraphableDataSeries) list = []
    
module DcaCalculation =
    open GenerateSeries
    let mutable dcaCalculation =
        { Start = DateTime.Now.Date.AddYears(-4)
          Duration = 5, Years
          Cadence = Weekly
          FiatAmount = 500m }
    
module PostgresDb =
    module UserTransactions =
        open Db.Postgres
        
        type UserTransactionDao =
            { Id: Guid
              UserId: string
              TransactionId: Guid }
        
        type TransactionDao =
            { Id: Guid
              Date: DateTime
              Type: string
              BtcAmount: decimal<btc>
              FiatAmount: decimal option
              FiatCode: string option }
                    
        let txToDao (tx: Transaction) =
            match tx with
            | Buy { Id = id; Date = dateTime; Amount = btcAmount; Fiat = fiatAmount } ->
                { Id = id
                  Date = dateTime
                  Type = "BUY"
                  BtcAmount = btcAmount.AsBtc
                  FiatAmount = Some fiatAmount.Amount
                  FiatCode = fiatAmount.Currency.ToString() |> Some }
            | Sell { Id = id; Date = dateTime; Amount = btcAmount; Fiat = fiatAmount }->
                { Id = id
                  Date = dateTime
                  Type = "SELL"
                  BtcAmount = btcAmount.AsBtc
                  FiatAmount = fiatAmount.Amount |> Some
                  FiatCode = fiatAmount.Currency.ToString() |> Some }
            | Income { Id = id; Date = dateTime; Amount = btcAmount } ->
                { Id = id
                  Date = dateTime
                  Type = "INCOME"
                  BtcAmount = btcAmount.AsBtc
                  FiatAmount = None
                  FiatCode = None }
            | Spend { Id = id; Date = dateTime; Amount = btcAmount } ->
                { Id = id
                  Date = dateTime
                  Type = tx.TxName
                  BtcAmount = btcAmount.AsBtc
                  FiatAmount = None
                  FiatCode = None }
                
        let txFromDao (dao: TransactionDao) =
            let parseFiat (fiat: string) =
                match fiat.ToLower() with
                | "usd" -> USD
                | "cad" -> CAD
                | "eur" -> EUR
                | _ -> failwith $"Unsupported fiat currency {fiat}"
                
            match dao.Type.ToLower() with
            | "buy" ->
                let fiat =
                    { Amount = dao.FiatAmount.Value
                      Currency = parseFiat dao.FiatCode.Value }
                Buy { Id = dao.Id; Date = dao.Date; Amount = (Btc dao.BtcAmount); Fiat = fiat }
            | "sell" ->
                let fiat =
                    { Amount = dao.FiatAmount.Value
                      Currency = parseFiat dao.FiatCode.Value }
                Sell { Id = dao.Id; Date = dao.Date; Amount = (Btc dao.BtcAmount); Fiat = fiat }
            | "income" ->
                Income { Id = dao.Id; Date = dao.Date; Amount = (Btc dao.BtcAmount) }
            | "spend" ->
                Spend { Id = dao.Id; Date = dao.Date; Amount = (Btc dao.BtcAmount) }
            | _ -> failwith $"Unsupported transaction type {dao.Type}"
        
        module Insert =
            let private insertQuery =
                """
                insert into transactions
                    (id, date, type, btc_amount, fiat_amount, fiat_code)
                values (@id, @date, @type, @btcAmount, @fiatAmount, @fiatCode)
                """
            let private insertUserTxQuery =
                """
                insert into user_transactions
                    (transaction_id, user_id)
                values (@transactionId, @userId)
                """
                
            let private txToProps (txDao: TransactionDao) = [
                    "id", Sql.uuid txDao.Id
                    "date", Sql.timestamp txDao.Date
                    "type", Sql.text txDao.Type
                    "btcAmount", Sql.decimal (decimal txDao.BtcAmount)
                    "fiatAmount", Sql.decimalOrNone txDao.FiatAmount
                    "fiatCode", Sql.textOrNone txDao.FiatCode
                ]
            let private insertUserToProps (txId, userId: Guid) = [
                    "transactionId", Sql.uuid txId
                    "userId", Sql.text (userId.ToString())
                ]
            
            let insertTx (tx: Transaction) (userId: Guid) =
                let txDao = txToDao tx
                task {
                    let! _ = postgresNonQuery insertQuery (txToProps txDao)
                    let! _ = postgresNonQuery insertUserTxQuery (insertUserToProps (txDao.Id, userId))
                    return ()
                }
                
            let insertMany (txs: Transaction list) (userId: Guid) =
                let txDaos = txs |> List.map txToDao
                
                let txsIdsAndUserIds = txDaos |> List.map (fun tx -> tx.Id) |> List.map (fun id -> id, userId)
                
                let insertManyTxs = [
                    insertQuery, (txDaos |> List.map txToProps)
                ]
                
                let insertManyUserTxs = [
                    insertUserTxQuery, txsIdsAndUserIds |> List.map insertUserToProps
                ]
                
                task {
                    let! _ = postgresManyNonQuery insertManyTxs
                    let! _ = postgresManyNonQuery insertManyUserTxs
                    return ()
                }
        module Update =
            let private updateTxQuery =
                """
                update transactions 
                set 
                    date = @date,
                    type = @type,
                    btc_amount = @btcAmount,
                    fiat_amount = @fiatAmount,
                    fiat_code = @fiatCode,
                    updated = now()
                where id = @id
                """.Trim()
                
            let private daoToProps (txDao: TransactionDao) = [
                "id", Sql.uuid txDao.Id
                "date", Sql.timestamp txDao.Date
                "type", Sql.text txDao.Type
                "btcAmount", Sql.decimal (decimal txDao.BtcAmount)
                "fiatAmount", Sql.decimalOrNone txDao.FiatAmount
                "fiatCode", Sql.textOrNone txDao.FiatCode
            ]
            
            let updateTx (t: Transaction) =
                let props = t |> txToDao |> daoToProps
                task {
                    let! updated = postgresNonQuery updateTxQuery props
                    return
                        match updated with
                        | 1 -> Some ()
                        | _ -> None
                }
                
        module Read =
            let getAllTxsForUser (userId: Guid) =
                let getTxsQuery =
                    """
                    select t.id, t.date, t.type, t.btc_amount, t.fiat_amount, t.fiat_code from user_transactions ut
                    inner join transactions t on t.id = ut.transaction_id
                    where user_id = @userId
                    order by t.date desc
                    """
                let queryParams = [ "userId", Sql.string (userId.ToString()) ]
                
                task {
                    let! txs =
                        postgresQuery getTxsQuery queryParams (fun row ->
                            { Id = row.uuid "id"
                              Date = row.dateTime "date"
                              Type = row.text "type"
                              BtcAmount = row.decimal "btc_amount" |> (*) 1.0m<btc>
                              FiatAmount = row.decimalOrNone "fiat_amount"
                              FiatCode = row.textOrNone "fiat_code" })
                    return txs |> List.map txFromDao
                }
                
            let private getTxsForUserQuery =
                """
                    select t.id, t.date, t.type, t.btc_amount, t.fiat_amount, t.fiat_code
                    from user_transactions ut
                    inner join transactions t on t.id = ut.transaction_id
                    where user_id = @userId and t.date >= @date
                    order by t.date desc                
                """
                
            let getTxsForUserInHorizon (userId: Guid) (horizon: DateTime) =
                task {
                    let! txsDao = postgresQuery
                                    getTxsForUserQuery
                                    [ "userId", Sql.string (userId.ToString())
                                      "date", Sql.date horizon ]
                                    (fun row ->
                                        { Id = row.uuid "id"
                                          Date = row.dateTime "date"
                                          Type = row.text "type"
                                          BtcAmount = row.decimal "btc_amount" |> (*) 1.0m<btc>
                                          FiatAmount = row.decimalOrNone "fiat_amount"
                                          FiatCode = row.textOrNone "fiat_code" })
                    return txsDao |> List.map txFromDao
                }
            
            
            let private getTxByIdQuery =
                """
                    select t.id, t.date, t.type, t.btc_amount, t.fiat_amount, t.fiat_code
                    from user_transactions ut
                    inner join transactions t on t.id = ut.transaction_id
                    where t.id = @id AND ut.user_id = @userId
                """
            
            let getTxById (userId: Guid) (txId: Guid) =
                task {
                    let! tx = postgresQuery
                                getTxByIdQuery
                                [ "id", Sql.uuid txId
                                  "userId", Sql.string (userId.ToString())]
                                (fun row ->
                                    { Id = row.uuid "id"
                                      Date = row.dateTime "date"
                                      Type = row.text "type"
                                      BtcAmount = row.decimal "btc_amount" |> (*) 1.0m<btc>
                                      FiatAmount = row.decimalOrNone "fiat_amount"
                                      FiatCode = row.textOrNone "fiat_code" })
                    return
                        match tx with
                        | [ t ] -> txFromDao t |> Some
                        | [  ] -> None
                        | txs -> txs.Head |> txFromDao |> Some
                }
        module Delete =
            let private deleteUserTxQuery =
                """
                delete from user_transactions
                where user_id = @userId AND transaction_id = @txId 
                """
            let private deleteTxQuery =
                """
                delete from transactions where id = @txId
                """
            
            let deleteTxForUser (txId: Guid) (userId: Guid) =
                task {
                    let! _ = postgresNonQuery deleteUserTxQuery [
                        "txId", Sql.uuid txId
                        "userId", Sql.string (userId.ToString())
                    ]
                    
                    let! _ = postgresNonQuery deleteTxQuery [                   
                        "txId", Sql.uuid txId 
                    ]
                    return ()
                }
    module Prices =
        open Db.Postgres
        
        type PriceAtDateDao =
            { Id: Guid
              Price: decimal
              Date: DateTime
              Currency: string }
            
        module Read =
            open Stacker.Calculate
            open System.Linq
                
            let getMostRecentPrice (db: PlebJournalDb) (currency: Fiat)  =
                task {
                    let! res =
                        db.Prices
                            .Where(fun p -> p.Currency = currency.ToString())
                            .OrderByDescending(fun p -> p.Date)
                            .FirstOrDefaultAsync()
                    return res |> Option.ofObj |> Option.map (fun p -> p.Date)
                }
                
            let private getPricesForCurrency  =
                """
                select date, price from prices
                where currency = @currency
                order by date asc
                """
                
            let getPrices (currency: Fiat) =
                postgresQuery
                   getPricesForCurrency [ "currency", Sql.string (currency.ToString()) ]
                   (fun row ->
                        { Price = row.decimal "price"
                          Date = row.dateTime "date" })
            
            let private getPriceAtDateQuery =
                """
                select price from prices
                where currency = @currency and date = @date
                """
                
            let getPriceAtDate (currency: Fiat) (date: DateOnly) =
                task {
                    let! res = postgresQuery
                                   getPriceAtDateQuery
                                   [ "currency", Sql.string (currency.ToString())
                                     "date", Sql.date date ]
                                   (fun a -> a.decimal "price")
                    return res.Head
                }
                
        
        module Insert =   
            let insertHistoricalPrices (db: PlebJournalDb) (prices: PriceAtDateDao list) =
                let toInsert = prices |> List.map (fun p ->
                    new Price(
                        Date = p.Date.ToUniversalTime(),
                        BtcPrice = p.Price,
                        Currency = p.Currency
                    ))
                task {
                    do! db.Prices.AddRangeAsync(toInsert)
                    let! _ = db.SaveChangesAsync()
                    return ()
                }
            
            let insertUnique (db: PlebJournalDb) (price: PriceAtDateDao) =
                let price = new Price(
                    Id = price.Id,
                    Date = price.Date.ToUniversalTime(),
                    BtcPrice = price.Price,
                    Currency = price.Currency)
                task {                    
                    let! p = db.Prices.FindAsync(price.Id)
                    match (Option.ofObj p) with 
                    | Some _ -> return ()
                    | None ->
                        let! _ = db.Prices.AddAsync(price)
                        let! _ = db.SaveChangesAsync()
                        return ()
                }
    module CurrentPrice =
        open System.Linq
        module Update =
            let upsertCurrentPrice (db: PlebJournalDb) (price: Prices.PriceAtDateDao) =
                task {
                    let! existing = db.CurrentPrices.Where(fun p -> p.Currency = price.Currency).FirstOrDefaultAsync()
                    match (Option.ofObj existing) with
                    | None ->
                        let toInsert = CurrentPrice(
                            BtcPrice = price.Price,
                            Currency = price.Currency,
                            Created = DateTime.UtcNow,
                            Updated = DateTime.UtcNow)
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
            let private getCurrentPriceQuery =
                """
                select price from current_prices where currency = @currency
                """
            
            let getCurrentPrice (fiat: Fiat) =
                task {
                    let! a = Postgres.postgresQuery
                                 getCurrentPriceQuery
                                 [ "currency", Sql.string (fiat.ToString()) ]
                                 (fun row -> row.decimal "price")
                    return a.Head
                }