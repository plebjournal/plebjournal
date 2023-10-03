module Stacker.Web.Views.Pages.Dashboard

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let scripts = [
    script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
    script [ _src "https://cdn.datatables.net/v/bs5/jq-3.7.0/dt-1.13.6/b-2.4.1/datatables.min.js" ] []
    script [ _src "https://cdn.ckeditor.com/ckeditor5/39.0.2/classic/ckeditor.js"] []
    script [ _src "/js/portfolio-summary-chart.js" ] []
    script [ _src "/js/fiat-value-chart.js" ] []
]

let dashboardPage =
    [
        div [ _class "row mb-4" ] [
            div [ _class "col" ] [
                h2 [ _class "page-title" ] [ str "Dashboard" ]
            ]
            div [ _class "col-auto" ] [
                div [ _class "btn-list" ] [
                    button [
                        _class "btn btn-primary d-sm-inline-block"
                        _hxTrigger "click"
                        _hxTarget "#modal-container"
                        _hxGet "/bought"
                    ] [
                        str "Enter Transaction"
                    ]
                    
                    button [
                        _class "btn btn-primary d-sm-inline-block"
                        _hxTrigger "click"
                        _hxTarget "#modal-container"
                        _hxGet "/take-a-note"
                    ] [
                        str "Take a Note"
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
