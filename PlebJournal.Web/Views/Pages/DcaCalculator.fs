module Stacker.Web.Views.Pages.DcaCalculator

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Views

let dcaCalculatorPage = [
    
    div [ _class "row mb-4" ] [
        div [ _class "col" ] [
            h2 [ _class "page-title" ] [ str "DCA Calculator" ]
        ]
    ]
    div [ _class "card" ] [
        div [ _class "col-md-12"; _hxGet "/charts/dca-calculator"; _hxTrigger "revealed" ] [ ]
    ]
    script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
]