module Stacker.Web.Views.Partials.Charts


open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Models
open Stacker.GenerateSeries

let wmaChartContainer =
    div
        [ _class "card" ]
        [ div [ _class "card-header" ] [ div [ _class "card-title" ] [ str "200 WMA" ] ]
          div
              []
              [ div [ _id "wma-chart-container" ] []
                script [ _src "/js/plotly-wma.js" ] [] ] ]

let dcaCalculatorChartContainer (dcaRequest: GenerateRequest) =
    div [ _class "card-body" ] [
        Forms.dcaCalculatorForm dcaRequest
        div [] [
            div [ _id "dca-calculator-container" ] []
            script [ _src "/js/dca-calculator.js" ] []
        ]
    ]

let workbenchChartContainer =
    div [ _class "card"; ] [
        div [ _class "card-header" ] [
            div [ _class "card-title" ] [ str "Formula Builder" ]
        ]
        div [ _class "card-body" ] [
            div [ ] [ Forms.workbenchFormulaDesigner None [] ]
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
                            _hxTrigger "change, tx-created, tx-deleted"
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
        ]
    ]