namespace Stacker

open System

module Charting =
    open Calculate
    module Domain =
        type TimeRangeDays = int
        type Ma =
            { Date: DateTime
              Price: decimal
              MA: decimal }

        type GraphPlot = { X: DateTime; Y: decimal }
        
        type PriceHistory =
            | BtcUsd

        type Formula =
            | Sma of GraphableDataSeries * TimeRangeDays
            | Sum of GraphableDataSeries * TimeRangeDays
            | Multiple of GraphableDataSeries * decimal

        and GraphableDataSeries =
            | Price of PriceHistory
            | Formula of Formula
                
    open Domain
    
    module DataProcessing =
        let rec evaluate (startingPriceSeries: PriceAtDate seq) (dataSource: GraphableDataSeries) : GraphPlot seq =
            match dataSource with
            | Price prices ->
                match prices with
                | BtcUsd ->
                    startingPriceSeries |> Seq.map (fun p -> { GraphPlot.X = p.Date; Y = p.Price })
            | Formula formula ->
                match formula with
                | Sma (dataSrc, timeRangeDays) ->
                    let data =
                        evaluate startingPriceSeries dataSrc
                        |> Seq.map (fun gp -> { Price = gp.Y; Date = gp.X })
                        |> Seq.toArray
                    ma data timeRangeDays
                    |> Seq.map (fun ma -> { X = ma.Date; Y = ma.MA })
                | Sum (dataSrc, timeRangeDays) ->
                    let data =
                        evaluate startingPriceSeries dataSrc
                        |> Seq.map (fun gp -> { Price = gp.Y; Date = gp.X })
                        |> Seq.toArray
                    sum data timeRangeDays
                    |> Seq.map (fun sm -> { X = sm.Date; Y = sm.Sum })
                | Multiple (dataSrc, mult) ->
                    let data =
                        evaluate startingPriceSeries dataSrc
                        |> Seq.map (fun gp -> { Price = gp.Y; Date = gp.X })
                        |> Seq.toArray
                    data |> Seq.map (fun d -> { X = d.Date; Y = (d.Price * mult) })          
    
    module Parsing =
        open FParsec
        
        let pDataSource, pDataSourceRef = createParserForwardedToRef()

        let pUsd = spaces >>. pstring "btc-usd" >>% BtcUsd

        let pSma = 
            (spaces >>. (pstring "sma"))
            .>>. (pchar '(' >>. spaces >>. pDataSource .>> spaces .>> pchar ',' .>> spaces)
            .>>. (pint32 .>> spaces .>> pchar ')' .>> spaces)
            |>> fun ((_, data), days) ->
                Sma(data, days)

        let pSum = 
            (spaces >>. (pstring "sum"))
            .>>. (pchar '(' >>. spaces >>. pDataSource .>> spaces .>> pchar ',' .>> spaces)
            .>>. (pint32 .>> spaces .>> pchar ')' .>> spaces)
            |>> fun ((_, data), days) ->
                Sum(data, days)

        let pMult = 
            (spaces >>. (pstring "mult"))
            .>>. (pchar '(' >>. spaces >>. pDataSource .>> spaces .>> pchar ',' .>> spaces)
            .>>. (pfloat .>> spaces .>> pchar ')' .>> spaces)
            |>> fun ((_, data), mult) ->
                Multiple(data, decimal mult)        

        let formula = choice [ pSma; pSum; pMult ]
        let priceHistory = choice [ pUsd ]

        let pformulaDataSource = formula |>> fun a -> Formula a
        let pPriceHistoryDataSource = priceHistory |>> fun p -> Price p

        pDataSourceRef.Value <- choice [
            pPriceHistoryDataSource
            pformulaDataSource
        ]
        
        let parse (input: string) = run pDataSource input