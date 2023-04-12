module Stacker.Web.Views.Pages.Transactions

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let transactionsPage = [
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
                    _class "btn btn-primary d-sm-none"
                    _hxTrigger "click"
                    _hxTarget "#modal-container"
                    _hxGet "/bought"                    
                ] [
                    i [ _class "ti ti-plus" ] []
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
]
