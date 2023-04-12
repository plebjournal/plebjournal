module Stacker.Web.Views.Partials.TxHistory

open System.Globalization
open Stacker.Calculate
open Stacker.Domain
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Models

let historyTable (txs: (Change option * Transaction) list) (selectedHorizon: TxHistoryHorizon option) =
    let changeColumn (change: Change option) =
        match change with
        | None -> div [] []
        | Some (Increase percent) ->
            div [ _style "color: green;" ] [ str $"{percent}%%" ]
        | Some (Decrease percent) ->
            div [] [ str $"{percent}%%" ]
    
    div
        [ _class "card"; _id "stacking-history" ]
        [ div [ _class "card-body border-bottom py-3" ] [
            div [ _class "row" ] [
                div [ _class "col text-muted" ] [
                    h3 [] [
                        str "Transactions"
                    ]
                ]
                div [ _class "col-auto ms-auto text-muted" ] [
                    select [
                        _name "horizon"
                        _type "button"
                        _class "form-select"
                        _hxGet "/history"
                        _hxTarget "#stacking-history"
                        _hxTrigger "change"
                    ] [
                        div [ _class "htmx-indicator" ] [ div [_class "spinner-border text-blue"] [] ]
                        option ([ _value "2-months" ] @ [ if selectedHorizon = Some TwoMonths then _selected ]) [ str "2 Months" ]
                        option ([ _value "12-months" ] @ [ if selectedHorizon = None || selectedHorizon = Some TwelveMonths then _selected ]) [str "12 Months"]
                        option ([ _value "24-months" ] @ [ if selectedHorizon = Some TwoYears then _selected ]) [ str "24 Months" ]
                        option ([ _value "all-data" ] @ [ if selectedHorizon = Some AllData then _selected ]) [ str "All Data" ]
                    ]
                ]
            ]
          ]
          div [ _class "table-responsive" ] [
                table [ _class "table card-table table-vcenter datatable" ] [
                    thead [] [
                        tr [] [ 
                            th [] [ str "Type" ]
                            th [] [ str "Amount" ]                                         
                            th [] [ str "Fiat" ]
                            th [] [ str "Fiat Rate" ]
                            th [] [ str "% change" ]
                            th [] [ str "Date" ]
                            th [] []
                        ]
                    ]
                    tbody
                        []
                        (txs
                         |> List.map (fun (change, tx) ->
                             tr
                                 []
                                 [                                           
                                   td [] [ str tx.TxName ]
                                   
                                   let fiatAmount =
                                       function
                                       | Some (f: FiatAmount) -> f.Amount.ToString("C2", CultureInfo.CreateSpecificCulture("en-US"))
                                       | None -> ""

                                   let fiatCurrent =
                                       function
                                       | Some f -> f.Currency.ToString()
                                       | None -> ""
                                       
                                   let btcAmount = tx.Amount |> decimal |> fun d -> d.ToString("F8")
                                   td [] [
                                       i [ _class "ti ti-coin-bitcoin text-yellow"; _alt "BTC" ] []
                                       str btcAmount
                                   ]

                                   td [] [ str $"{fiatAmount tx.Fiat} {fiatCurrent tx.Fiat}" ]
                                   let pricePerCoin =
                                       tx.PricePerCoin
                                       |> Option.map (fun (d, fiat) ->
                                           d
                                           |> fun d -> d.ToString("C2", CultureInfo.CreateSpecificCulture("en-US"))
                                           |> fun str -> $"{str} - {fiat.ToString()}")
                                       
                                       |> Option.defaultValue ""
                                   td [] [ str $"{pricePerCoin}" ]
                                   td [ ] [ changeColumn change ]
                                   td [] [ str $"{tx.DateTime.ToShortDateString()}" ]
                                   td [] [
                                       div [ _class "nav-item dropdown" ] [
                                            a [ _class "nav-link"; _href "#"; _data "bs-toggle" "dropdown" ] [
                                                i [ _class "ti ti-pencil" ] []
                                            ]
                                            div [ _class "dropdown-menu dropdown-menu-end dropdown-menu-arrow" ] [
                                               a [
                                                    _class "dropdown-item"
                                                    _href "#"
                                                    _hxTrigger "click"
                                                    _hxTarget "#modal-container"
                                                    _hxGet $"/tx/details/{tx.Id}"
                                                ] [ str "Details" ]
                                               a [
                                                   _class "dropdown-item"
                                                   _href "#"
                                                   _hxTrigger "click"
                                                   _hxTarget "#modal-container"
                                                   _hxGet $"/tx/edit/{tx.Id}"
                                               ] [ str "Edit" ]
                                               a [
                                                   _href "#"
                                                   _class "dropdown-item"
                                                   _hxTrigger "click"
                                                   _hxTarget "#modal-container"
                                                   _hxGet $"/tx/delete/{tx.Id}"
                                               ] [ str "Delete" ]
                                            ]
                                        ]
                                   ]
                                    ])) ] ] ]
