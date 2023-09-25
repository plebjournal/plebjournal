module Stacker.Web.Views.Pages.Index

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let indexPage =
    [
        div [ _class "bd-masthead mb-3" ] [
            str "Stack Sats"
        ]
        div [ _class "row text-center" ] [
            div [ _class "col" ] [
                h1 [ _class "page-title h1" ] [ str "PlebJournal" ]
            ]
            div [ _class "col text-secondary" ] [
                a [ _href "/login" ] [ str "login" ]
            ]
        ]
    ]
