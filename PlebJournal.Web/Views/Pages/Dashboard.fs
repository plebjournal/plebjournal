module Stacker.Web.Views.Pages.Dashboard

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let scripts = [
    script [ _src "https://cdn.plot.ly/plotly-2.16.1.min.js" ] []
    script [ _src "https://cdn.jsdelivr.net/npm/apexcharts" ] []
    script [ _src "https://cdn.ckeditor.com/ckeditor5/39.0.2/classic/ckeditor.js"] []
    script [ _src "/js/fiat-value-apex-chart.js" ] []
]

let dashboardPage =
    [
        div [ _class "row mb-4" ] [
            div [ _class "col-auto" ] [
                div [ _class "btn-list" ] [
                    button [
                        _class "btn btn-primary d-sm-inline-block"
                        _hxTrigger "click"
                        _hxTarget "#modal-container"
                        _hxGet "/tx/new"
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
            div [ _class "card" ] [
                div [ _class "card-body" ] [
                    h2 [] [ str "Fiat Value by Day" ]
                    div [ _class "row" ] [
                        div [ _class "btn-list" ] [
                            button [ _class "btn"; _id "chart-1-month" ] [ str "1 Month" ]
                            button [ _class "btn"; _id "chart-6-months" ] [ str "6 Months" ]
                            button [ _class "btn"; _id "chart-1-year" ] [ str "1 Year" ]
                            button [ _class "btn"; _id "chart-all-time"] [ str "All Data" ]
                        ]    
                    ]                    
                ]
                div [ _id "fiat-value-apex-chart"; ] [  ]
            ]
        ]
    ] @ scripts
