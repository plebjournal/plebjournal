module Stacker.Web.Views.Partials.Widgets

open System.Globalization
open Stacker.Calculate
open Giraffe.ViewEngine
open Stacker.Web.Models

let btcBalance balance (change: Change option) =
    let percentChangeSpan (change) =
        match change with
        | Increase percent ->
            span [ _class "text-green d-inline-flex lh-1"; _title "6 Mo" ] [ str $"+{percent}%%" ]
        | Decrease percent ->
            span [ _class "text-red d-inline-flex lh-1"; _title "6 Mo" ] [ str $"-{percent}%%" ]

    div
        [ _class "card" ]
        [ div
              [ _class "card-body" ]
              [ div [ _class "d-flex align-items-center" ]
                    [ div [ _class "subheader" ] [ str "Balance" ]
                       ]
                div [ _class "d-flex align-items-baseline" ] [
                    let balanceStr = balance.Total |> decimal |> fun d -> d.ToString("F8")
                    h1 [ _class "h1 mb-3 me-2" ] [ str $"{balanceStr} BTC" ]
                ]
                match change with
                | Some v ->                   
                    div [ _class "mb-2" ] [
                        str "Change in last 6 months: "
                        percentChangeSpan v
                    ]
                | None -> div [] [] ] ]

let fiatValue (fiatBalance: FiatBalanceViewModel) =
    let nguSpan change =
        match change with
        | None -> span [] []
        | Some ngu when ngu >= 0.0m ->
            span [ _class "text-green d-inline-flex lh-1"; _title "NgU" ] [ str $"+{ngu}x" ]
        | Some ngu ->
            span [ _class "text-red d-inline-flex lh-1"; _title "NgU" ] [ str $"-{ngu}x" ]
    
    div
        [ _class "card" ]
        [ div
              [ _class "card-body" ]
              [ div [ _class "subheader" ] [ str "Fiat Value" ]
                
                let value = fiatBalance.CurrentValue |> decimal

                div [ _class "d-flex align-items-baseline" ] [
                    div [ _class "h1 mb-3 me-3" ] [
                        let culture = CultureInfo.CreateSpecificCulture("en-US")
                        let valueStr = value.ToString("C2", culture)
                        str $"{valueStr} {fiatBalance.Fiat}"
                    ]
                ]

                div [ _class "mb-2" ] [
                    str "NgU: "; nguSpan fiatBalance.Ngu
                ]
            ]
        ]

let btcPrice { Price = price; Fiat = fiat } =
    let culture = CultureInfo.CreateSpecificCulture("en-US")

    div [ _class "card" ] [
        div [ _class "card-body" ] [
            div [ _class "subheader" ] [
                str "Btc Price"
            ]
            div [ _class "d-flex align-items-baseline" ] [
                div [ _class "h1 mb-1" ] [
                    let valueStr = price.ToString("C2", culture)
                    str $"{valueStr} {fiat}"
                ]
            ]
            div [ _id "btc-price-card-sparkline" ] [ ]
        ]
        script [ _src "/js/btc-price-sparkline.js" ] []
    ]