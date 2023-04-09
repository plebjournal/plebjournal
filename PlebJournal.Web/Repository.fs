module Stacker.Web.Repository

open System
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
            let private getMostRecentQuery =
                """
                select date from prices
                where currency = @currency            
                order by date desc limit 1
                """
                
            let getMostRecentPrice (currency: Fiat) =
                task {
                    let! res = postgresQuery getMostRecentQuery [ "currency", Sql.string (currency.ToString()) ]
                                (fun row -> row.dateTime "date")
                    return res |> List.tryHead
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
            let private insertQuery =
                """
                insert into prices
                    (id, date, price, currency)
                values 
                    (@id, @date, @price, @currency)
                """
                
            let private findHistoricalPriceQuery =
                """
                    select id from prices where id = @id
                """
            let private insertPriceToProps (price: PriceAtDateDao) =
                [
                    "id", Sql.uuid price.Id
                    "date", Sql.date price.Date.Date
                    "price", Sql.decimal price.Price
                    "currency", Sql.string price.Currency
                ]
            
            let insertHistoricalPrice (price: PriceAtDateDao) =
                task {
                    let! _ = postgresNonQuery insertQuery (insertPriceToProps price)
                    return ()
                }
                
            let insertHistoricalPrices (prices: PriceAtDateDao list) =
                task {
                    let! _ = postgresManyNonQuery [
                        insertQuery, (prices |> List.map insertPriceToProps)
                    ]
                    return ()
                }
            
            let insertUnique (price: PriceAtDateDao) =
                let queryProps = [
                    "id", Sql.uuid price.Id
                ]
                task {
                    let! existing =
                        postgresQuery findHistoricalPriceQuery queryProps (fun r -> r.uuid "id")
                    match existing with
                    | [] -> do! insertHistoricalPrice price
                    | _ -> return ()
                }
    module CurrentPrice =
        module Update =
            let upsertCurrentPriceQuery =
                """
                insert into current_prices
                    (id, price, currency)
                values 
                    (@id, @price, @currency)
                on conflict (currency)
                do 
                    update
                        set price = @price,
                            updated = now()
                """
                
            let upsertCurrentPrice (price: Prices.PriceAtDateDao) =
                task {
                    let! a = Postgres.postgresNonQuery upsertCurrentPriceQuery [
                        "id", Sql.uuid price.Id
                        "price", Sql.decimal price.Price
                        "currency", Sql.string price.Currency
                    ]
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