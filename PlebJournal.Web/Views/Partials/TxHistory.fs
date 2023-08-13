module Stacker.Web.Views.Partials.TxHistory

open System.Globalization
open Stacker.Calculate
open Stacker.Domain
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Models

let historyTable (txs: TxHistoryViewModel list) =            
    let nguColumn (ngu: NgU option) =
        match ngu with
        | None -> div [] []
        | Some d when d >= 1.0m ->
            div [ _style "color: green;" ] [ str $"{d}x" ]
        | Some d ->
            div [ _style "color: red;" ] [ str $"{d}x" ]
    
    div
        [ _class "card"; _id "stacking-history" ]
        [ 
          div [ _class "card-body" ] [
                table [ _id "tx-table"; _class "table table-vcenter" ] [
                    thead [] [
                        tr [] [ 
                            th [] [ str "Type" ]
                            th [] [ str "Amount" ]                                         
                            th [] [ str "Fiat" ]
                            th [] [ str "NgU" ]
                            th [] [ str "Date" ]
                            th [] []
                        ]
                    ]
                    tbody
                        []
                        (txs
                         |> List.map (fun {Transaction = tx; PercentChange = change; Ngu = ngu} ->
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
                                   td [] [ nguColumn ngu ]
                                   td [] [ str (tx.DateTime.ToString("yyyy-MM-dd")) ]
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
                                    ])) ] ]
          script [ _src "/js/tx-history-datatable.js" ] []]
          
