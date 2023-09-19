module Stacker.Web.Views.Partials.User

open System
open Giraffe.ViewEngine

let userNav (user: string option) =
    match user with
    | None -> a [ _href "/login" ] [ str "Login" ]
    | Some null -> a [ _href "/login" ] [ str "Login" ]
    | Some usr ->
        let usrName = usr.Substring(0, Math.Min(usr.Length, 25))

        let roboImage = $"https://robohash.org/{usr}.png"
        div [ _class "nav-item dropdown" ] [
            a [
                _href "#"
                _class "nav-link d-flex lh-1 text-reset p-0"
                _data "bs-toggle" "dropdown"
            ] [
                span [
                    _class "avatar"
                    _style $"background-image: url({roboImage})"
                ] []
                div [ _class "d-none d-xl-block ps-2" ] [
                    div [] [ str usrName ]
                ]
            ]
            div [ _class "dropdown-menu dropdown-menu-end dropdown-menu-arrow" ] [
                a [ _href "/settings"; _class "dropdown-item" ] [ str "Settings" ]
                a [ _href "/logout"; _class "dropdown-item" ] [ str "Logout" ]
            ]
        ]