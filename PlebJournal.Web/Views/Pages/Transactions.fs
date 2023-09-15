module Stacker.Web.Views.Pages.Transactions

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let scripts = [
    script [ _src "https://cdn.datatables.net/v/bs5/jq-3.7.0/dt-1.13.6/b-2.4.1/datatables.min.js" ] []
]

let transactionsPage =
    [
    div [ _class "row mb-4" ] [
        div [ _class "col" ] [
            h2 [ _class "page-title" ] [ str "Stacking History" ]
        ]
        div [ _class "col-auto" ] [
            div [ _class "btn-list" ] [
                button [
                    _class "btn btn-primary d-none d-sm-inline-block"
                    _hxTrigger "click"
                    _hxTarget "#modal-container"
                    _hxGet "/bought"
                ] [
                    str "Enter Transaction"
                ]
                button [
                    _class "btn d-none d-sm-inline"
                    _hxTarget "#modal-container"
                    _hxGet "/import"
                    _hxTrigger "click"
                ] [
                    str "Import"
                ]
            ]
        ]
    ]

    div [ _class "row" ] [
        div [ _class "col-sm-12" ] [
            div [
                _hxGet "/history"
                _hxTrigger "revealed, bought from:body, tx-deleted from:body, tx-updated from:body"
            ] []
        ]
    ]
    
    ] @ scripts