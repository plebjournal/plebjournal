module Stacker.Web.Views.Partials.Widgets

open System.Globalization
open Stacker.Calculate
open Stacker.Domain
open Giraffe.ViewEngine
open Stacker.Web.Models

let btcBalance balance (cadValue: decimal<btc> option) change =
    let percentChangeSpan (change) =
        match change with
        | None -> span [] []
        | Some (Increase percent) ->
            span [ _class "text-green d-inline-flex lh-1"; _title "6 Mo" ] [ str $"+{percent}%%" ]
        | Some (Decrease percent) ->
            span [ _class "text-red d-inline-flex lh-1"; _title "6 Mo" ] [ str $"-{percent}%%" ]

    div
        [ _class "card" ]
        [ div
              [ _class "card-body" ]
              [ div [ _class "d-flex align-items-center" ]
                    [ div [ _class "subheader" ] [ str "BTC Balance" ]
                       ]
                div [ _class "d-flex align-items-baseline" ] [
                    let balanceStr = balance.Total |> decimal |> fun d -> d.ToString("F8")
                    div [ _class "h1 mb-3 me-2" ] [ str $"{balanceStr} BTC" ]
                    div [ _class "me-auto" ] [
                        percentChangeSpan change
                    ]
                ]
                match cadValue with
                | Some v ->
                    let culture = CultureInfo.CreateSpecificCulture("en-US")
                    let value = v |> decimal |> (fun d -> d.ToString("C2", culture))
                    div [ _class "subheader" ] [
                        str $"${value} CAD"
                    ]
                | None -> div [] [] ] ]

let fiatValue balance (cadValue: decimal<btc> option) change =
    let percentChangeSpan (change) =
        match change with
        | None -> span [] []
        | Some (Increase percent) ->
            span [ _class "text-green d-inline-flex lh-1"; _title "7D" ] [ str $"+{percent}%%" ]
        | Some (Decrease percent) ->
            span [ _class "text-red d-inline-flex lh-1"; _title "7D" ] [ str $"-{percent}%%" ]
    
    div
        [ _class "card" ]
        [ div
              [ _class "card-body" ]
              [ div [ _class "subheader" ] [ str "Fiat Value" ]
                match cadValue with
                | Some v ->
                    let value = v |> decimal

                    div [ _class "d-flex align-items-baseline" ] [
                        div [ _class "h1 mb-3 me-3" ] [
                            let culture = CultureInfo.CreateSpecificCulture("en-US")
                            let valueStr = value.ToString("C2", culture)
                            str $"{valueStr} CAD"
                        ]
                        div [ _class "me-auto" ] [
                            percentChangeSpan change
                        ]
                    ]

                | None -> div [] []
                div [ _class "subheader" ] [
                    let balanceStr = balance.Total |> decimal |> fun d -> d.ToString("F8")
                    str $"{balanceStr} BTC"
                ]
            ]
        ]

let btcPrice (cad: decimal, usd: decimal) =
    let culture = CultureInfo.CreateSpecificCulture("en-US")

    div [ _class "card" ] [
        div [ _class "card-body" ] [
            div [ _class "subheader" ] [
                str "Btc Price"
            ]
            div [ _class "d-flex align-items-baseline" ] [
                div [ _class "h1 mb-3 me-3" ] [
                    let valueStr = cad.ToString("C2", culture)
                    str $"{valueStr} CAD"
                ]
            ]
            div [ _class "subheader" ] [
                    let valueStr = usd.ToString("C2", culture)
                    str $"{valueStr} USD"
            ]
        ]
    ]