module Stacker.Web.Views.Layout

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let private htmlHead =
    head
        []
        [ meta [ _charset "utf-8" ]
          meta [ _name "viewport"; _content "width=device-width, initial-scale=1, viewport-fit=cover" ]
          link [ _rel "icon"; _type "image/x-icon"; _href "/img/favicon.png" ]
          link
              [ _rel "stylesheet"
                _href "https://cdn.jsdelivr.net/npm/@tabler/core@latest/dist/css/tabler.min.css" ]
          link
              [ _rel "stylesheet"
                _href "https://cdn.jsdelivr.net/npm/@tabler/icons@latest/iconfont/tabler-icons.min.css" ]
          link [ _rel "stylesheet"; _href "https://cdn.datatables.net/v/dt/dt-1.13.6/datatables.min.css" ]
          link [ _rel "stylesheet"; _href "/css/style.css" ]
          script
              [ _src "https://unpkg.com/htmx.org@1.8.4"
                _integrity "sha384-wg5Y/JwF7VxGk4zLsJEcAojRtlVp1FKKdGy1qN+OMtdq72WRvX/EdRdqg/LOhYeV"
                _crossorigin "anonymous" ]
              []
          script [ _src "/js/htmx.js" ] []
          script
              [ _type "module"
                _src "https://cdn.jsdelivr.net/npm/@tabler/core@latest/dist/js/tabler.min.js" ]
              []
          script [ _src "/js/toast.js" ] []
          script [ _src "/js/modal-helper.js" ] []
          title [] [ str "Pleb Journal" ] ]


let private appHeader =
    header [ _class "navbar navbar-expand-md navbar-light" ] [
        div [ _class "container" ] [
            button [
                _class "navbar-toggler"
                _type "button"
                _data "bs-toggle" "collapse"
                _data "bs-target" "#navbar-menu"
            ] [
                span [ _class "navbar-toggler-icon" ] []
            ]
            h1 [ _class "navbar-brand" ] [
                a [ _href "/" ] [ str "Pleb Journal" ]
            ]
            div [ _class "navbar-nav flex-row order-md-last" ] [
                div [ _class "nav-item"
                      _hxGet "/nav/user"
                      _hxTrigger "revealed" ] [
                    a [ _href "/login" ] [ str "Login" ]
                ]
            ]
        ]
    ]
    
let private splashHeader =
    let gitIcon =
        rawText """
            <svg xmlns="http://www.w3.org/2000/svg" class="icon icon-tabler icon-tabler-brand-github" width="24" height="24" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" fill="none" stroke-linecap="round" stroke-linejoin="round">
               <path stroke="none" d="M0 0h24v24H0z" fill="none"></path>
               <path d="M9 19c-4.3 1.4 -4.3 -2.5 -6 -3m12 5v-3.5c0 -1 .1 -1.4 -.5 -2c2.8 -.3 5.5 -1.4 5.5 -6a4.6 4.6 0 0 0 -1.3 -3.2a4.2 4.2 0 0 0 -.1 -3.2s-1.1 -.3 -3.5 1.3a12.3 12.3 0 0 0 -6.2 0c-2.4 -1.6 -3.5 -1.3 -3.5 -1.3a4.2 4.2 0 0 0 -.1 3.2a4.6 4.6 0 0 0 -1.3 3.2c0 4.6 2.7 5.7 5.5 6c-.6 .6 -.6 1.2 -.5 2v3.5"></path>
            </svg>
            """
    
    header [ _class "header" ] [
        div [ _class "container" ] [
            nav [ _class "row items-center" ] [
                div [ _class "col" ] [
                    h1 [ _class "navbar-brand" ] [
                        a [ _href "/"; _class "navbar-link" ] [ str "Pleb Journal" ]    
                    ]
                ]
                div [ _class "col-auto ml-auto" ] [
                    a [ _href "https://github.com/plebjournal/plebjournal" ] [
                        gitIcon
                    ]
                ]
                div [ _class "col-auto ml-auto" ] [
                    a [ _href "/login"; _class "nav-link" ] [ str "Login" ]
                ]
                div [ _class "col-auto ml-auto" ] [
                    a [ _href "/create-account"; _class "nav-link" ] [ str "Sign Up" ]
                ]
            ]
        ]
    ]

let private navbar =
    div
        [ _class "navbar-expand-md" ]
        [ div
              [ _class "collapse navbar-collapse"; _id "navbar-menu" ]
              [ div
                    [ _class "navbar navbar-light" ]
                    [ div
                          [ _class "container" ]
                          [ ul
                                [ _class "navbar-nav" ]
                                [ li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/dashboard" ]
                                            [ span [ _class "nav-link-title" ] [ str "Dashboard" ] ] ]
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/transactions" ]
                                            [ span [ _class "nav-link-title" ] [ str "Stacking History" ] ] ]
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/notes" ]
                                            [ span [ _class "nav-link-title" ] [ str "Notes" ] ] ]
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/blockchaininfo" ]
                                            [ span [ _class "nav-link-title" ] [ str "Blockchain Info" ] ] ]                                  
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/dca-calculator" ]
                                            [ span [ _class "nav-link-title" ] [ str "DCA Calculator" ] ] ]
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/indicators" ]
                                            [ span [ _class "nav-link-title" ] [ str "Indicators" ] ] ]
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/work-bench" ]
                                            [ span [ _class "nav-link-title" ] [ str "Workbench" ] ] ]
                                   ] ] ] ] ]

let private toast =
    div [ _class "position-fixed position-fixed bottom-0 end-0 p-3" ] [
    div [
          _id "toast"
          _class "toast"
          ] [
            
        div [
            _class "alert alert-success"
            _style "margin-bottom: 0px"
        ] [
            h4 [ _id "toast-body-header"; _class "alert-title" ] [ str "Wow! Congratulations on buying BTC" ]
            div [ _class "text-muted" ] [ str "Transaction saved successfully" ]
        ]
    ]
]

        
let onload (js: string) =
    KeyValue ("onload",js)
    
let withLayout (pageContent: XmlNode list) =
    html [] [
        htmlHead
        body [
            onload "configureHtmx()"
        ] [
            div [ _class "page" ] [
                appHeader
                navbar
                div [ _class "page-wrapper" ] [
                    div [ _id "modal-container" ] []
                    div [ _class "page-body" ] [ div [ _class "container" ] pageContent ]
                    toast
                ]
            ]
        ]
    ]

let withLayoutNoHeader (pageContent: XmlNode list) =
    html [] [
        htmlHead
        body [
            onload "configureHtmx()"
        ] [
            div [ _class "page" ] [
                div [ _class "page-wrapper" ] [
                    div [ _class "page-body" ] [
                        div [ _class "container" ]
                            (splashHeader :: pageContent)
                    ]
                ]
            ]
        ]
    ]