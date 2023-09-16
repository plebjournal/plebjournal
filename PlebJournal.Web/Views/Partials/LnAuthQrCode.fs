module Stacker.Web.Views.Partials.LnAuthQrCode

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Stacker.Web.Models

let lnAuthQrCode (qrCode: LnAuthQrCode) =
    div [ _class "row" ] [
        div [ _class "col-sm-0 col-md-4" ] []
        div [ _class "col-sm-12 col-md-4" ] [
            div [ _class "card" ] [
                div [ _class "card-body" ] [
                    h1 [ _class "h2 text-center" ] [ str "Login with Lightning ⚡" ]
                    div [ _class "text-center" ] [
                        img [ _src $"data:image/png;base64,{qrCode.QrCodeData}"; _class "mb-3" ]
                    ]
                    
                    p [ _class "p text-center" ] [ str "Scan this QR code with your lightning wallet. No identifying information needed, not even an email." ]
                    p [ _class "p text-center" ] [
                        a [ _href "https://lightninglogin.live/learn" ] [ str "Learn more" ]
                        str " about LNURL-auth"
                    ]
                ]
            ]
        ]
        div [ _class "col-sm-0" ] []
        div [
            _hxGet $"/login/lnauth/check?k1={qrCode.K1}"
            _hxTrigger "every 2s"
            _hxSwap "none"
        ] []
    ]
