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
            div [ _class "col-auto" ] [
                button [
                    _class "btn btn-primary"
                    _hxTrigger "click"
                    _hxTarget "#modal-container"
                    _hxGet "/take-a-note"
                ] [
                    str "Take a Note"
                ]
            ]
        ]
        div [
            _hxGet "/notes-list"
            _hxTrigger "revealed, note-created from:body"
        ] []
    ] @ scripts