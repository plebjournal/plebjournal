[<AutoOpen>]
module Stacker.Web.Json
open System.IO
open System.Text.Json

type Json() =
    static member camelCase = JsonSerializerOptions(JsonSerializerDefaults.Web)

    static member deserialize<'t>(body: string) =
        JsonSerializer.Deserialize<'t>(body, options = Json.camelCase)
        
    static member deserialize<'t>(body: Stream) =
        JsonSerializer.Deserialize<'t>(body, options = Json.camelCase)        