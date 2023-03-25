module Stacker.Web.Views.Pages.Login

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let loginForm =
    form [
        _id "login"
        _hxPost "/login"
        _hxSwap "outerHTML"
    ] [
        div [ _class "mb-3" ] [
            div [ _class "mb-3" ] [
                label [ _class "form-label"; _required ] [ str "Username" ]
                input [ _required; _class "form-control"; _name "Username"; _type "text"; ]
            ]
            div [ ] [
                label [ _class "form-label"; _required ] [ str "Password" ]
                input [ _required; _class "form-control"; _name "Password"; _type "password"; ]            
            ]
        ]
        div [ _class "row mb-3" ] [
            div [ _class "col" ] [
                a [ _href "/"; _class "btn btn-secondary" ] [ str "Cancel" ]
            ]
            div [ _class "col-auto" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [ str "Login" ]
            ]       
        ]
    ]

let loginPage =
    [
        div [ _class "row mb-4" ] [
            div [ _class "col" ] [
                h2 [ _class "page-title" ] [ str "Login" ]
            ]
        ]
        
        div [ _class "row-mb-4" ] [
            div [ _class "col-sm-12 col-md-6" ] [
                div [ _class "card" ] [
                    div [ _class "card-body" ] [
                        loginForm
                        div [ _class "row" ] [
                            div [ _class "col-sm-6" ] [
                                a [ _href "/create-account" ] [ str "Create Account" ]
                            ]
                        ]
                    ]
                ]    
            ]
        ]
    ]