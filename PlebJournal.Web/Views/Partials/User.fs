module Stacker.Web.Views.Partials.User

open Giraffe.ViewEngine

let userNav (user: string option) =
    match user with
    | None -> a [ _href "/login" ] [ str "Login" ]
    | Some usr ->
        div [ _class "nav-item dropdown" ] [
            a [ _class "nav-link"; _href "#"; _data "bs-toggle" "dropdown" ] [
                str usr
            ]
            div [ _class "dropdown-menu dropdown-menu-end dropdown-menu-arrow" ] [
                a [ _href "#"; _class "dropdown-item" ] [ str "Settings" ]
                a [ _href "/logout"; _class "dropdown-item" ] [ str "Logout" ]
            ]
        ]