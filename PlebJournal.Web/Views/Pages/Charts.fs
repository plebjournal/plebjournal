module Stacker.Web.Views.Pages.Charts

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let scripts = [
    script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
    script [ _src "https://cdn.jsdelivr.net/npm/apexcharts" ] []
    script [ _src "/js/portfolio-summary-chart.js" ] []
    script [ _src "/js/fiat-value-chart.js" ] []
]

let chartsPage =
    [
        div [ _class "row mb-4" ] [
            div [
                _class "col-md-12"
                _hxGet "/chart"
                _hxTrigger "revealed, tx-created from:body, tx-deleted from:body, tx-updated from:body"
            ] [ ]
        ]
        
        div [ _class "row mb-4" ] [
            div [
                _class "col-md-12"
                _hxGet "/charts/fiat-value"
                _hxTrigger "revealed, tx-created from:body, tx-deleted from:body, tx-updated from:body" ] []
        ]
    ] @ scripts