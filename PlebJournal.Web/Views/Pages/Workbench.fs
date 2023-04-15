module Stacker.Web.Views.Pages.Workbench

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let workbenchPage =
    [
        div [ _class "row" ] [
            div [ _class "col-sm-12"; _hxGet "/workbench-chart"; _hxTrigger "revealed" ] [ ]
        ]
        script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
    ]