module Stacker.Web.Views.Pages.Login

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let loginForm (errMsg: string option) =
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
        match errMsg with
        | None -> div [] []
        | Some msg -> 
            div [ _class "row mb-3" ] [
                div [  _class "show invalid-feedback" ] [
                    str msg
                ]
            ]
        div [ _class "row mb-3" ] [
            div [ _class "loading-spinner"; _class "col" ] [
                div [ _class "spinner-border text-blue"; ] []    
            ]
            
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
            div [ _class "col-md-12 col-lg-6" ] [
                div [ _class "card" ] [
                    div [ _class "card-body" ] [
                        loginForm None
                        div [ _class "row" ] [
                            div [ _class "col" ] [
                                a [ _href "/create-account" ] [ str "Create Account" ]
                            ]
                            div [ _class "col-auto" ] [
                                a [ _href "/login/lnauth" ] [ str "Login with Lightning" ]
                            ]
                        ]
                    ]
                ]    
            ]
        ]
    ]
    
let lnAuthPage =
    [        
        div [
            _hxGet "/login/lnauth/qrcode"
            _hxTrigger "revealed"
            _hxSwap "outerHTML"
        ] []
    ]