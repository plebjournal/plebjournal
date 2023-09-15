module Stacker.Web.Views.Partials.Notes

open System
open System.Globalization
open Stacker.Domain
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let notesList (notes: Note seq) =
    let sentiment (s: Sentiment option) =
        s |> Option.map string |> Option.defaultValue ""
        
    let take56Chars(s: string) =
        s
        |> Option.ofObj
        |> Option.map (fun s ->
            let take = Math.Min(s.Length, 56)
            s.Substring(0, take))
        |> Option.map (fun trimmed ->
            if s.Length > 56 then $"{trimmed}..." else trimmed)
        |> Option.defaultValue ""
    
    div [ _class "row" ] (notes |> Seq.toList |> List.map (fun n ->
        div [ _class "col-sm-12 col-md-4 mb-3" ] [
            div [ _class "card card-sm" ] [
                a [
                    _href "#"
                    _class "card-link"
                    _hxGet $"/notes/{n.Id}"
                    _hxTrigger "click"
                    _hxTarget "#modal-container"
                ] [
                    div [ _class "card card-body" ] [
                        h3 [ _class "h1" ] [
                            n.Date.ToString("yyyy-MM-dd") |> fun s -> $"{s} " |> str
                        ]
                        div [ _class "row" ] [
                            div [ _class "col" ] [
                                span [ _class "badge badge-outline text-blue" ] [ n.Sentiment |> sentiment |> str ]
                            ]
                            div [ _class "col-auto" ] [
                                div [ _class "text-secondary" ] [
                                    let culture = CultureInfo.CreateSpecificCulture("en-US")
                                    let valueStr = n.BtcPrice.ToString("C2", culture)
                                    
                                    span [ _class "badge bg-yellow bg-yellow-lt" ] [ str $"{valueStr} {n.Fiat}"  ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]))
    