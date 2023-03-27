namespace Stacker

open System
open Stacker.Domain
open Stacker.ExtendedTypes

module Calculate =
    type PriceAtDate = { Date: DateTime; Price: decimal }

    type MA =
        { Date: DateTime
          Price: decimal
          MA: decimal }

    type Sum =
        { Date: DateTime
          Price: decimal
          Sum: decimal }

    let twoDecimals d = Decimal.Round(d, 2)
    let oneDecimal d = Decimal.Round(d, 1)

    let foldTxs (txs: Transaction seq) =
        txs
        |> Seq.fold
            (fun s tx ->
                match tx with
                | Spend (_, btcAmount) -> s - btcAmount.AsBtc
                | Income (_, btcAmount) -> s + btcAmount.AsBtc
                | Buy (_, b, _) -> s + b.AsBtc
                | Sell (_, b, _) -> s - b.AsBtc)
            0.0m<btc>

    let foldDailyTransactions (txs: Transaction seq) =
        txs
        |> Seq.map (fun tx -> tx.DateTime.Date, tx)
        |> Seq.groupBy fst
        |> Seq.map (fun (d, txsSameDay) ->
            let dayTotal = txsSameDay |> Seq.map snd |> foldTxs
            d, dayTotal)

    let movingSumOfTxs (txs: (DateTime * decimal<btc>) seq) =
        txs
        |> Seq.sortBy fst
        |> Seq.fold
            (fun s tx ->
                let date, amount = tx

                match s with
                | [] -> [ date, amount ]
                | head :: _ ->
                    let _, lastAmount = head
                    (date, amount + lastAmount) :: s)
            []
        |> Seq.sortBy fst

    let portfolioHistoricalValue (txs: Transaction list) (historicalUsd: PriceAtDate array) =
        if txs.IsEmpty then [] else
        let dayTotals = txs |> foldDailyTransactions |> Seq.toList

        let movingSumOfTotals = dayTotals |> movingSumOfTxs |> Seq.toList

        let startingDate, _ = movingSumOfTotals.Head

        let relevantPrices =
            historicalUsd
            |> Array.where (fun p -> p.Date >= startingDate)
            |> Array.sortBy (fun p -> p.Date)

        let pricesAndMaybeTransactions =
            relevantPrices
            |> Array.map (fun p ->
                let portfolioHit = movingSumOfTotals |> List.tryFind (fun (d, _) -> d.Date = p.Date)
                p, portfolioHit)
            |> Array.sortBy (fun (p, _) -> p.Date)

        let pricesAndPortfolioTotals =
            Seq.fold
                (fun state el ->
                    match state with
                    | [] ->
                        let price, portfolio = el

                        match portfolio with
                        | None -> failwith "Something went wrong"
                        | Some (_, amount) -> [ (price, amount) ]
                    | head :: _ ->
                        let price, portfolio = el

                        match portfolio with
                        | None ->
                            let _, amount = head
                            (price, amount) :: state
                        | Some (_, amount) -> (price, amount) :: state)
                []
                pricesAndMaybeTransactions

        pricesAndPortfolioTotals
        |> List.map (fun (p, amount) -> (p, amount, p.Price * amount |> decimal))
        |> List.sortBy (fun (d, _, _) -> d)

    let ma (historicalUsd: PriceAtDate array) (days) =

        let averagePrice (prices: PriceAtDate seq) =
            prices |> Seq.map (fun p -> p.Price) |> Seq.average

        [ 0 .. historicalUsd.Length - 1 ]
        |> Seq.fold
            (fun s (idx: int) ->
                let maybeSlice = Array.tryGetRange (idx - days) idx historicalUsd

                let wma =
                    match maybeSlice with
                    | None -> None
                    | Some arr ->
                        let a =
                            { MA = arr |> averagePrice |> twoDecimals
                              Price = historicalUsd[idx].Price |> twoDecimals
                              Date = historicalUsd[idx].Date }

                        Some a

                match s with
                | [] -> [ wma ]
                | _ -> wma :: s)
            []
        |> Seq.somes

    let sum (historical: PriceAtDate array) days =
        let sum (prices: PriceAtDate seq) = prices |> Seq.sumBy (fun p -> p.Price)

        [ 0 .. historical.Length - 1 ]
        |> Seq.fold
            (fun s (idx: int) ->
                let maybeSlice = Array.tryGetRange (idx - days) idx historical

                let wma =
                    match maybeSlice with
                    | None -> None
                    | Some arr ->
                        let a =
                            { Sum = arr |> sum |> twoDecimals
                              Price = historical[idx].Price |> twoDecimals
                              Date = historical[idx].Date }

                        Some a

                match s with
                | [] -> [ wma ]
                | _ -> wma :: s)
            []
        |> Seq.somes

    let movingCostBasis (txs: Transaction list) =
        txs
        |> List.map (fun tx -> tx.DateTime, tx.Fiat)
        |> List.filter (fun (d, fiat) -> fiat.IsSome)
        |> List.fold
            (fun s (d, el) ->
                match s with
                | [] -> [ d, el.Value.Amount ]
                | lst ->
                    let _, headValue = s.Head
                    let next = d, headValue + el.Value.Amount
                    next :: lst)
            []

    type Change =
        | Increase of decimal
        | Decrease of decimal

    let numericalChange a b =
        if a = 0.0m || b = 0.0m then
            None
        else if b >= a then
            let increase = b - a
            increase / a |> (*) 100m |> oneDecimal |> Increase |> Some
        else
            let decrease = a - b
            decrease / a |> (*) 100m |> oneDecimal |> Decrease |> Some

    let percentChange (currentPrice: decimal) (tx: Transaction) =
        tx.PricePerCoin
        |> Option.bind (fun (purchasePrice, _) -> numericalChange purchasePrice currentPrice)