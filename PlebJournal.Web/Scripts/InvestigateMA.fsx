open System
open System.IO
open System.Text.Json

type PriceAtDateDao =
    { Id: Guid
      Price: decimal
      Date: DateTime
      Currency: string }

let parseHistorical () =
    let camelCase = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let raw = File.ReadAllText("./historical-usd-prices.json")
    JsonSerializer.Deserialize<PriceAtDateDao array>(raw, options = camelCase)
    |> Array.sortBy (fun p -> p.Date)
    
let prices = parseHistorical ()

let dups =
    prices
    |> Array.groupBy (fun p -> p.Date)
    |> Array.where ( fun (d, p) -> p.Length > 1)
    |> Array.map (fun (d,p) -> p[0])
    
printfn "no of dups: %i" dups.Length