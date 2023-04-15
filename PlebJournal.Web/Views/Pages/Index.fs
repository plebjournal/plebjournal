module Stacker.Web.Views.Pages.Index

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let scripts = [
    script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
    script [ _src "/js/portfolio-summary-chart.js" ] []
    script [ _src "/js/fiat-value-chart.js" ] []
]

let indexPage =
    [
        div [ _class "row mb-4" ] [
            div [ _class "col" ] [
                h2 [ _class "page-title" ] [ str "Dashboard" ]
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
        div [ _class "row mb-4 row-cards" ] [       
            div [ _class "col-sm-12 col-md-6 col-lg-4"; _id "balance-container" ] [
                div [ _hxGet "/balance"; _hxTrigger "revealed, tx-created from:body" ] [ ]
            ]
            div [ _class "col-sm-12 col-md-6 col-lg-4"; _id "fait-value-container" ] [
                div [
                    _hxGet "/fiat-value"
                    _hxTrigger "revealed, tx-created from:body, tx-deleted from:body, tx-updated from:body"
                ] [ ]
            ]
            div [ _class "col-sm-12 col-md-6 col-lg-4" ] [
                div [
                    _hxGet "/btc-price"
                    _hxTrigger "revealed, every 5m"
                ] [ ]
            ]
        ]
        
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
        
        div [ _class "row" ] [
            div [ _class "col-sm-12" ] [
                div [
                    _hxGet "/history"
                    _hxTrigger "revealed, tx-created from:body, tx-deleted from:body, tx-updated from:body"
                ] []
            ]
        ]
    ] @ scripts
