module Stacker.Web.Views.Pages.Settings

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let settingsPage = [
    div [ _class "row mb-4" ] [
        div [ _class "col" ] [
            h2 [ _class "page-title" ] [ str "Settings" ]
        ]
    ]
    
    div [ _class "card" ] [
        div [
            _class "card-body"
            _hxGet "/settings/user-settings"
            _hxTrigger "revealed"
        ] []
    ]
]
