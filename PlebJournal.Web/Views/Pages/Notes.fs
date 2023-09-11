module Stacker.Web.Views.Pages.Notes

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let notesPage = [
    div [ _class "row mb-4" ] [
        div [ _class "col" ] [
            h2 [ _class "page-title" ] [ str "Notes" ]
        ]
    ]
    div [
        _hxGet "/notes-list"
        _hxTrigger "revealed"
    ] []
]