module Stacker.Web.Views.Pages.Twitter

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

let twitterPage = [
    div [ _class "row" ] [
        div [ _class "col-sm-12 col-md-6" ] [
            rawText
                """
                <a class="twitter-timeline" href="https://twitter.com/lopp/lists/bitcoin">A Twitter List by lopp</a> <script async src="https://platform.twitter.com/widgets.js" charset="utf-8"></script>
                """            
        ]
    ]
]