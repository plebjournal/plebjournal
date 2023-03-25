module Stacker.Web.Views.Pages.Transactions

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let transactionsPage =
    [
        div [ _class "row" ] [
            div [ _class "col-sm-12" ] [
                div [ _hxGet "/history"; _hxTrigger "revealed, bought from:body" ] []
            ]
        ]
    ]
