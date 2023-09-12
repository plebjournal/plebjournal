module Stacker.Web.Views.Pages.Notes

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let scripts = [
    script [ _src "https://cdn.ckeditor.com/ckeditor5/39.0.2/classic/ckeditor.js"] []    
]

let notesPage =
    [
        div [ _class "row mb-4" ] [
            div [ _class "col" ] [
                h2 [ _class "page-title" ] [ str "Notes" ]
            ]
        ]
        div [
            _hxGet "/notes-list"
            _hxTrigger "revealed"
        ] []
    ] @ scripts