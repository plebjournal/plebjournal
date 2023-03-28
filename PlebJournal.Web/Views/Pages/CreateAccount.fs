module Stacker.Web.Views.Pages.CreateAccount

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Models

let createAccountForm (errs: CreateAccountErrors option) =
    let usernameErrors = errs.IsSome && errs.Value.Username.IsSome
    let passwordErrors = errs.IsSome && errs.Value.Password.IsSome
    let identityErrors = errs.IsSome && errs.Value.Identity.Length > 0
    form [
        _hxPost "/create-account"
        _hxSwap "outerHTML"
        _hxTarget "this"
    ] [
        div [ _class "mb-3" ] [
            div [ _class "mb-3" ] [
                label [ _class "form-label required"; _required ] [ str "Username" ]
                input [
                    _required
                    _name "Username"
                    _type "text"
                    _minlength "6"
                    if usernameErrors then
                        _class "form-control is-invalid"
                    else _class "form-control"
                ]
                if usernameErrors then
                    div [ _class "invalid-feedback" ] [ str errs.Value.Username.Value ]
                else div [] []
            ]
            div [ _class "mb-3" ] [
                label [ _class "form-label required"; _required ] [ str "Password" ]
                input [
                    _required
                    _name "Password"
                    _type "password"
                    if passwordErrors || identityErrors then _class "form-control is-invalid" else _class "form-control"
                ]
                if passwordErrors || identityErrors then
                    div [ _class "invalid-feedback" ]
                        [
                            yield! (if passwordErrors then [ str errs.Value.Password.Value ] else [])
                            yield! (errs.Value.Identity |> List.map str)
                        ]
                else div [] []
            ]
            div [ _class "mb-3" ] [
                label [ _class "form-label required"; _required ] [ str "Repeat Password" ]
                input [ _required; _class "form-control"; _name "PasswordRepeat"; _type "password"; ]
                div [] [  ]
            ]
            div [ _class "mb-3" ] [
                label [ _class "form-label required"; _required;  ] [ str "Default Fiat" ]
                select [ _type "button"; _name "Fiat"; _required; _class "form-select" ] [
                    option [ _value "USD"; _selected ] [ str "USD" ]
                    option [ _value "CAD"; ] [ str "CAD" ]
                    option [ _value "EUR"; ] [ str "EUR" ]
                ]
            ]
            
        ]
        div [ _class "row mb-3" ] [
            div [ _class "loading-spinner"; _class "col" ] [
                div [ _class "spinner-border text-blue"; ] []    
            ]
            div [ _class "col" ] [
                a [ _href "/login"; _class "btn btn-secondary" ] [ str "Cancel" ]
            ]
            div [ _class "col-auto" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [ str "Create Account" ]
            ]       
        ]
    ]


let createAccountPage =
    [
        div [ _class "row mb-4" ] [
            div [ _class "col" ] [
                h2 [ _class "page-title" ] [ str "Create Account" ]
            ]
        ]
        
        div [ _class "row-mb-4" ] [
            div [ _class "col-md-12 col-lg-6" ] [
                div [ _class "card" ] [
                    div [ _class "card-body" ] [
                        createAccountForm None
                    ]
                ]    
            ]
        ]
    ]    