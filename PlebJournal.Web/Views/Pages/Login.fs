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
                input [ _required; _class "form-control"; _placeholder "stacker@plebs.com"; _name "Username"; _type "text"; ]
            ]
            div [ _class "mb-3"] [
                label [ _class "form-label"; _required ] [ str "Password" ]
                input [ _required; _class "form-control"; _name "Password"; _type "password"; ]            
            ]
            div [ _class "mb-2" ] [
                label [ _class "form-check" ] [
                    input [ _type "checkbox"; _class "form-check-input"; _name "Remember"; _value "true"; _checked ]
                    span [ _class "form-check-label" ] [ str "Remember me" ]
                ]
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
        div [ _class "form-footer" ] [
            button [ _type "submit"; _class "btn btn-primary w-100" ] [ str "Sign in" ]
        ]
        div [ _class "row mb-3 text-center" ] [
            div [ _class "loading-spinner"; _class "col" ] [
                div [ _class "spinner-border text-blue"; ] []    
            ]            
        ]
        div [ _class "text-center text-secondary mt-3" ] [
            str "Don't have an account? "
            a [ _href "/create-account" ] [ str "Sign up" ]
        ]
    ]
    
let loginPage =
    [
        div [ _class "container container-tight" ] [
            div [ _class "text-center" ] [
                h1 [ _class "h1" ] [ str "Pleb Journal" ]
            ]
            div [ _class "card card-md" ] [
                div [ _class "card-body" ] [
                    h2 [ _class "h2 text-center mb-4" ] [ str "Login to your account" ]
                    loginForm None
                ]
                div [ _class "hr-text" ] [ str "OR" ]
                div [ _class "card-body" ] [
                    div [ _class "row" ] [
                        div [ _class "col text-center" ] [
                            a [ _href "/login/lnauth"; _class "btn" ] [ str "⚡️ Login with Lightning" ]
                        ]
                    ]
                ]
            ]
        ]        
    ]
    
let lnAuthPage =
    [
        div [ _class "container container-tight" ] [
            div [ _class "text-center" ] [
                h1 [ _class "h1" ] [ str "Pleb Journal" ]
            ]
            div [
            _hxGet "/login/lnauth/qrcode"
            _hxTrigger "revealed"
            _hxSwap "outerHTML"
        ] []
        ]
    ]