module Stacker.Web.Views.Partials

open System
open Stacker.Calculate
open Stacker.Charting.Domain
open Stacker.Domain
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Models
open Stacker.GenerateSeries

let userNav (user: string option) =
    match user with
    | None -> a [ _href "/login" ] [ str "Login" ]
    | Some usr ->
        div [ _class "nav-item dropdown" ] [
            a [ _class "nav-link"; _href "#"; _data "bs-toggle" "dropdown" ] [
                str usr
            ]
            div [ _class "dropdown-menu dropdown-menu-end dropdown-menu-arrow" ] [
                a [ _href "#"; _class "dropdown-item" ] [ str "Settings" ]
                a [ _href "/logout"; _class "dropdown-item" ] [ str "Logout" ]
            ]
        ]

let importForm (errs: string list) =
    div [ _class "modal"; _id "import-form" ] [
        div [ _class "modal-dialog" ] [
            div [ _class "modal-content" ] [
                div [ _class "modal-header" ] [
                    h1 [ _class "modal-title" ] [ str "Import Transactions" ]
                ]
                div [ _class "modal-body" ] [
                    div [] (errs |> List.map (fun err -> p [] [str err]))
                    form [
                        _enctype "multipart/form-data"
                        _hxPost "/import"
                        _hxTarget "#import-form"
                        _hxSwap "outerHTML"
                    ] [
                        div [ _class "row mb-3" ] [
                            div [ _class "col-sm-12 col-md-6" ] [
                                label [ _class "form-label"; _required; ] [ str "CSV File" ]
                                input [ _type "file"; _name "CsvFile"; _required; _class "form-control" ]
                                input [ _type "hidden"; _name "Other"; _value "Testing" ]
                            ]
                        ]
                        div [ _class "row" ] [
                            div
                                [ _class "col" ]
                                [ button
                                      [ _class "btn btn-secondary"; _data "bs-dismiss" "modal" ]
                                      [ str "Cancel" ] ]
                            div
                                [ _class "col-auto" ]
                                [ button
                                      [ _type "submit"
                                        _class "btn btn-success"
                                        _data "bs-dismiss" "modal" ]
                                      [ str "Import" ]
                                    ]
                            ]
                    ]
                ]
            ]
        ]
    ]
    
let txsForm =
    form
        [ _hxPost "/bought"
          _hxTarget "this"
          _hxSwap "outerHTML" ]
            [ div
                  [ _class "row mb-3" ]
                  [
                    div [ _class "col-sm-12 col-md-4" ] [
                        label [ _class "form-label"; _required;  ] [ str "Tx" ]
                        select [ _type "button"; _name "Type"; _required; _class "form-select" ] [
                            option [ _value "Buy"; _selected ] [ str "Buy" ]
                            option [ _value "Sell"; ] [ str "Sell" ]
                            option [ _value "Income"; ] [ str "Income" ]
                            option [ _value "Spend"; ] [ str "Spend" ]
                        ]
                    ]
                    
                    div
                        [ _class "col-sm-12 col-md-6" ]
                        [
                          label [ _class "form-label"; _required; _min "0" ] [ str "Btc Amount" ]
                          div [ _class "input-group" ] [
                              input [ _name "btcAmount"; _required; _class "form-control" ]
                              select [ _name "btcUnit"; _required; _class "form-select" ] [
                                  option [ _value "Btc"; _selected ] [ str "BTC" ]
                                  option [ _value "Sats"; ] [ str "SATS" ]
                              ]
                          ]
                        ]
                  ]
              div [ _class "row mb-3" ] [
                    div
                        [ _class "col-sm-12 col-md-4" ]
                        [ label [ _class "form-label"; _required; _min "0" ] [ str "Fiat Amount" ]
                          input [ _name "fiatAmount"; _required; _class "form-control" ] ]
                    div
                        [ _class "col-sm-12 col-md-4" ]
                        [ label [ _class "form-label"; _required; _min "0" ] [ str "Fiat Currency" ]
                          select
                              [ _type "text"; _name "fiat"; _required; _class "form-select" ]
                              [ option [ _value "CAD" ] [ str "CAD" ]
                                option [ _value "USD" ] [ str "USD" ] ] ] ]

              div
                  [ _class "mb-3" ]
                  [ label [ _class "form-label" ] [ str "Date" ]
                    input [
                        _type "datetime-local"
                        _name "date"
                        _required
                        _class "form-control"
                        _value (DateTime.Now.ToString("yyyy-MM-ddTHH:mm"))
                    ] ]

              div
                  [ _class "row" ]
                  [ div
                        [ _class "col" ]
                        [ button
                              [ _class "btn btn-secondary"; _onclick "closeModal()" ]
                              [ str "Cancel" ] ]
                    div
                        [ _class "col-auto" ]
                        [ button
                              [ _type "submit"
                                _class "btn btn-success"
                                ]
                              [ str "Save" ]
                          ] ] ]
    
let boughtBtcModal =
    div [] [
        div [
            _id "modal-backdrop"
            _class "modal modal-backdrop fade show"
            _style "display:block;"
            _onclick "closeModal()"
        ] []
        div
            [ _class "modal show modal-blur"
              _id "bought-btc-form"
              _style "display:block;"
              _tabindex "-1" ] [
                div [ _class "modal-dialog" ] [
                    div
                        [ _class "modal-content"; ]
                        [
                            div [ _class "modal-header" ] [
                                h1 [ _class "modal-title" ] [ str "Enter Transaction" ]
                                button [ _type "button"; _class "btn-close"; _onclick "closeModal()" ] []
                            ]
                            div [ _class "modal-body" ] [
                                txsForm
                            ] ] ]
                   ]
    ]

let txToast () =
    div [ _class "position-fixed  bottom-0 end-0 p-3" ] [
        div [ _class "alert alert-success"; ] [
            h4 [ _class "alert-title" ] [ str "Wow! Congratulations on buying BTC" ]
            div [ _class "text-muted" ] [ str "Transaction saved successfully" ]
        ]    
    ]
    
        
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
        [ div [ _class "card-header" ] [ h3 [ _class "card-title" ] [ str "Stacking History" ] ]
          div [ _class "card-body border-bottom py-3" ] [
            div [ _class "row" ] [
                div [ _class "col text-muted" ] [ str "Show 10" ]
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
                                       | Some (f: FiatAmount) -> f.Amount.ToString()
                                       | None -> ""

                                   let fiatCurrent =
                                       function
                                       | Some f -> f.Currency.ToString()
                                       | None -> ""
                                   td [] [ i [ _class "ti ti-coin-bitcoin text-yellow"; _alt "BTC" ] []; str $"{tx.Amount}" ]

                                   td [] [ str $"{fiatAmount tx.Fiat} {fiatCurrent tx.Fiat}" ]
                                   let pricePerCoin =
                                       tx.PricePerCoin
                                       |> Option.map (fun (d, fiat) -> $"{d |> fun d -> Decimal.Round(d, 2)} - {fiat.ToString()}")
                                       |> Option.defaultValue ""
                                   td [] [ str $"{pricePerCoin}" ]
                                   td [ ] [ changeColumn change ]
                                   td [] [ str $"{tx.DateTime.ToShortDateString()}" ]
                                    ])) ] ] ]

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
                    div [ _class "h1 mb-3 me-2" ] [ str $"{balance.Total} BTC" ]
                    div [ _class "me-auto" ] [
                        percentChangeSpan change
                    ]
                ]
                match cadValue with
                | Some v ->
                    let value = v |> decimal |> (fun d -> d.ToString("F"))
                    div [ _class "subheader" ] [ str $"${value} CAD" ]
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
                    let value = v |> decimal |> fun d -> d.ToString("C")

                    div [ _class "d-flex align-items-baseline" ] [
                        div [ _class "h1 mb-3 me-3" ] [ str $"{value} CAD" ]
                        div [ _class "me-auto" ] [
                            percentChangeSpan change
                        ]
                    ]

                | None -> div [] []
                div [ _class "subheader" ] [ str $"{balance.Total} BTC" ]

                ] ]

let wmaChartContainer =
    div
        [ _class "card" ]
        [ div [ _class "card-header" ] [ div [ _class "card-title" ] [ str "200 WMA" ] ]
          div
              []
              [ div [ _id "wma-chart-container" ] []
                script [ _src "/js/plotly-wma.js" ] [] ] ]
        
let dcaCalculatorForm (generateDca: GenerateRequest) =
    let date = generateDca.Start.Date.ToShortDateString()
    let cadenceOptionDropdown (name: string) =
        option ([ _value name; ] @ if name = string generateDca.Cadence then [ _selected ] else []) [ str name ]
        
    let durationOptionDropdown (name: string) =
        let _, durationUnit = generateDca.Duration
        option ([ _value name; ] @ if name = string durationUnit then [ _selected ] else []) [ str name ]        
        
    form [
        _hxPost "/dca-calculator"
        _id "dca-calculator-form"
        _hxTarget "#dca-calculator-form"
        _hxSwap "outerHTML"
    ] [
        div [ _class "row mb-3" ] [
            div [ _class "col-sm-6 col-md-2" ] [
                label [ _class "form-label" ] [ str "Start Date" ]
                input [ _name "StartDate"; _type "date"; _class "form-control"; _value date ]
            ]
            div [ _class "col-sm-6 col-md-2" ] [
                label [ _class "form-label" ] [ str "DCA Amount" ]
                input [ _name "Amount"; _type "number"; _class "form-control"; _value $"{generateDca.FiatAmount}" ]
            ]
            div [ _class "col-sm-6 col-md-2" ] [
                label [ _class "form-label" ] [ str "Cadence" ]
                select [ _name "Cadence"; _class "form-control form-select" ] [
                    cadenceOptionDropdown "Daily"
                    cadenceOptionDropdown "Weekly"
                    cadenceOptionDropdown "Monthly"
                ]
            ]
            div [ _class "col-sm-6 col-md-3" ] [
                label [ _class "form-label" ] [ str "Duration" ]
                div [ _class "row" ] [
                    div [ _class "col" ] [
                        input [ _name "Duration"; _type "number"; _class "form-control"; _value $"{generateDca.Duration |> fst}" ]
                        
                    ]
                    div [ _class "col" ] [
                        select [ _name "DurationUnit"; _class "form-control form-select" ] [
                            durationOptionDropdown "Days"
                            durationOptionDropdown "Weeks"
                            durationOptionDropdown "Months"
                            durationOptionDropdown "Years"
                        ]
                    ]
            
                    

                ]
            ]
        ]
        div [ _class "row" ] [
            div [ _class "col-sm-3" ] [
                button [ _class "btn btn-primary"; _type "submit" ] [ str "Calculate" ]    
            ]
        ]
    ]

        
let dcaCalculatorChartContainer (dcaRequest: GenerateRequest) =
    div [ _class "card-body" ] [
        dcaCalculatorForm dcaRequest
        div [] [
            div [ _id "dca-calculator-container" ] []
            script [ _src "/js/dca-calculator.js" ] []
        ]
    ]

let workbenchFormulaDesigner (formulaValue: string option) (graphableSeries: (string * GraphableDataSeries) list) =
    div [ _id "formula-designer" ] [
        let v = Option.defaultValue "" formulaValue
        div [ _class "row mb-3" ] [
            div [ _class "btn-list" ] [
                button [ _class "btn" ] [
                    str "BTC Price  "
                    span [ _class "badge bg-yellow ms-2" ] [ str "btc-usd" ]
                ]
            ]
        ]
        
        div [] (graphableSeries |> List.map (fun (name, g) ->
            div [] [
                span [] [ str $"{name} {(g.ToString())}" ]
            ]))
        form [ _id "workbench-formula-form"; _hxPost "/workbench/formula"; _hxTarget "#formula-designer"; _hxSwap "outerHTML" ] [
            div [ _class "row mb-3" ] [
                div [ _class "col col-md-3" ] [
                    label [ _class "form-label" ] [ str "formula name" ]
                    input [ _class "form-control"; _name "formulaName"; _placeholder "200 wma"; _required ]
                ]
                div [ _class "col col-md-3" ] [
                    label [ _class "form-label" ] [ str "formula" ]
                    input [ _class "form-control"; _name "formula"; _required; _placeholder "sma(btc-usd, 700)" ]
                ]
            ]
            button [ _type "submit"; _class "btn btn-primary" ] [ str "Save and Draw" ]
            
        ]
    ]

let workbenchChartContainer =
    div [ _class "card"; ] [
        div [ _class "card-header" ] [
            div [ _class "card-title" ] [ str "Formula Builder" ]
        ]
        div [ _class "card-body" ] [
            div [ ] [ workbenchFormulaDesigner None [] ]
        ]
        div [] [
            div [ _id "workbench-chart-container" ] []
            script [ _src "/js/workbench-chart.js" ] []
        ]
    ]

let chartContainer (selectedHorizon: TxHistoryHorizon option) =
    let createOption (value: string) (name: string) (selected: bool) =
        option ([ _value value ] @ [ if selected then _selected ]) [ str name ]
    
    div [
        _class "card"
        _id "portfolio-chart-container"
    ] [
        div [ _class "card-body border-bottom py-3" ] [
            div [ _class "row" ] [
                div [ _class "col" ] [
                    h3 [ _class "card-title" ] [str "Btc Portfolio"]
                ]
                div [ _class "col-auto ms-auto" ] [
                    div [ _class "col-auto ms-auto text-muted" ] [
                        select [
                            _name "horizon"
                            _type "button"
                            _class "form-select"
                            _hxGet "/chart"
                            _hxTrigger "change, tx-created"
                            _hxTarget "#portfolio-chart-container"
                            _hxSwap "outerHTML"
                        ] [
                            div [ _class "htmx-indicator" ] [ div [_class "spinner-border text-blue"] [] ]
                            createOption "2-months" "2 Months" (selectedHorizon = Some TwoMonths)
                            createOption "12-months" "12 Months" (selectedHorizon.IsNone || selectedHorizon = Some TwelveMonths)
                            createOption "24-months" "24 Months" (selectedHorizon = Some TwoYears)
                            createOption "all-data" "All Data" (selectedHorizon = Some AllData)
                        ]
                    ]
                ]
            ]
        ]
        div [ _id "chart-container" ] [
            div [ _id "portfolio-chart"; ] []
        ]
    ]

let fiatChartContainer (selectedHorizon: TxHistoryHorizon option) =
    let createOption (value: string) (name: string) (selected: bool) =
        option ([ _value value ] @ [ if selected then _selected ]) [ str name ]

    
    div [
        _class "card"
        _id "fiat-chart-container"
    ] [
        div [ _class "card-body body-border-bottom py-3" ] [
            div [ _class "row" ] [
                div [ _class "col" ] [
                    h3 [ _class "card-title" ] [ str "Fiat Value" ]
                ]
                div [ _class "col-auto ms-auto" ] [
                    div [ _class "text-muted" ] [
                        select [
                            _name "horizon"
                            _type "button"
                            _class "form-select"
                            _hxGet "/charts/fiat-value"
                            _hxTrigger "change, tx-created"
                            _hxTarget "#fiat-chart-container"
                            _hxSwap "outerHTML"
                        ] [
                            div [ _class "htmx-indicator" ] [ div [_class "spinner-border text-blue"] [] ]
                            createOption "2-months" "2 Months" (selectedHorizon = Some TwoMonths)
                            createOption "12-months" "12 Months" (selectedHorizon.IsNone || selectedHorizon = Some TwelveMonths)
                            createOption "24-months" "24 Months" (selectedHorizon = Some TwoYears)
                            createOption "all-data" "All Data" (selectedHorizon = Some AllData)                            
                        ]
                    ]
                ]
            ]
            div [ _id "fiat-value-chart"] []
            // script [ _src "/js/fiat-value-chart.js" ] []
        ]
    ]