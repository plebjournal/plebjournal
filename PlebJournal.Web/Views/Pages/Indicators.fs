module Stacker.Web.Views.Pages.Indicators

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let indicatorsPage =
    [
        div [ _class "row" ] [
            div [ _class "col-sm-12"; _hxGet "/wma-chart"; _hxTrigger "revealed" ] [ ]
        ]
        script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
    ]
