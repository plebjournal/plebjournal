open System
open System.IO
open System.Text.Json

type PriceAtDate = { Date: DateTime; Price: decimal }


type PriceAtDateDao =
    { Id: Guid
      Price: decimal
      Date: DateTime
      Currency: string }

let historicalPrices =
    let raw = File.ReadAllText("./historical-usd.json")
    JsonSerializer.Deserialize<PriceAtDate array>(raw)

printfn $"loaded prices {historicalPrices.Length}"

let mapped =
    historicalPrices
    |> Array.map (fun price ->
        {| Id = Guid.NewGuid()
           BtcPrice = price.Price
           Date = DateTime.SpecifyKind(price.Date, DateTimeKind.Utc)
           Currency = "USD" |})

printfn $"mapped prices. {mapped.Length}"

let serialized =
    let camelCase = JsonSerializerOptions(JsonSerializerDefaults.Web)
    JsonSerializer.Serialize(mapped, camelCase)
    
printfn "serialized to string"

File.WriteAllText("./historical-usd-prices.json", serialized)

printfn "Done writing file"
