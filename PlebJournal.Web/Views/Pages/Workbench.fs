module Stacker.Web.Views.Pages.Workbench

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let workbenchPage =
    [
        div [ _class "row" ] [
            div [ _class "col-sm-12"; _hxGet "/workbench-chart"; _hxTrigger "revealed" ] [ ]
        ]
    ]