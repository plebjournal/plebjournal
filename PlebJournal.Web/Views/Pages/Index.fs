module Stacker.Web.Views.Pages.Index

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine
    
let indexPage = [
    div [ _id "modal-container" ] []
    div [ _hxGet "/import"; _hxTrigger "revealed"] []
    
    div [ _class "row mb-4" ] [
        div [ _class "col" ] [
            h2 [ _class "page-title" ] [ str "Dashboard" ]
        ]
        div [ _class "col-auto" ] [
            button [ _class "btn btn-primary"
                     _hxTrigger "click"
                     _hxTarget "#modal-container"
                     _hxGet "/bought"
                     ] [
                str "Enter Transaction"
            ]
        ]
        div [ _class "col-auto" ] [
            button [ _class "btn"
                     _data "bs-toggle" "modal"
                     _data "bs-target" "#import-form" ] [
                str "Import"
                
            ]
        ]
    ]
    div [ _class "row mb-4 row-cards" ] [       
        div [ _class "col-sm-12 col-md-6 col-lg-4"; _id "balance-container" ] [
            div [ _hxGet "/balance"; _hxTrigger "revealed, tx-created from:body" ] [ ]
        ]
        div [ _class "col-sm-12 col-md-6 col-lg-4"; _id "fait-value-container" ] [
            div [ _hxGet "/fiat-value"; _hxTrigger "revealed, tx-created from:body" ] [ ]
        ]
    ]
    
    div [ _class "row mb-4" ] [
        div [ _class "col-md-12"; _hxGet "/chart"; _hxTrigger "revealed" ] [ ]
    ]
    
    div [ _class "row mb-4" ] [
        div [ _class "col-md-12"; _hxGet "/charts/fiat-value"; _hxTrigger "revealed" ] []
    ]
    
    div [ _class "row" ] [
        div [ _class "col-sm-12" ] [
            div [ _hxGet "/history"; _hxTrigger "revealed, tx-created from:body" ] []
        ]
    ]
        
    script [ _src "/js/bought-btc-form-modal.js" ] []
]
