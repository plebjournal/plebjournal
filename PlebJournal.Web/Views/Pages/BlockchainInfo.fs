module Stacker.Web.Views.Pages.BlockchainInfo

open Giraffe.ViewEngine.Htmx
open Giraffe.ViewEngine

let blockchainInfoPage =
    [
        div [ _class "row" ] [
            div [ _class "col-sm-12" ] [
                div [
                    _hxGet "/epochs"
                    _hxTrigger "revealed" ]
                    []
            ]
        ]
    ]
