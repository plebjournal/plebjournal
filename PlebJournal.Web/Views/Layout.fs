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


let private header =
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
                                            [ _class "nav-link"; _href "/" ]
                                            [ span [ _class "nav-link-title" ] [ str "Dashboard" ] ] ]
                                  li
                                      [ _class "nav-item" ]
                                      [ a
                                            [ _class "nav-link"; _href "/transactions" ]
                                            [ span [ _class "nav-link-title" ] [ str "Stacking History" ] ] ]
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
                header
                navbar
                div [ _class "page-wrapper" ] [
                    div [ _id "modal-container" ] []
                    div [ _class "page-body" ] [ div [ _class "container" ] pageContent ]
                    toast
                    
                ]
            ]
        ]
    ]
