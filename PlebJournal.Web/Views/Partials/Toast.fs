module Stacker.Web.Views.Partials.Toast

open Giraffe.ViewEngine

let txToast () =
    div [ _class "position-fixed  bottom-0 end-0 p-3" ] [
        div [ _class "alert alert-success"; ] [
            h4 [ _class "alert-title" ] [ ]
            div [ _class "text-muted" ] [ str "Transaction saved successfully" ]
        ]    
    ]
