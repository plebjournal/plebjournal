open System
open System.IO
open System.Text.Json
#r "nuget: NodaTime, 3.1.9"
open NodaTime
open NodaTime.TimeZones

let tzs =
    TimeZoneInfo.GetSystemTimeZones()
    |> Seq.map (fun tz -> {| Id = tz.Id; Name = tz.DisplayName |})
    |> Seq.toArray
    
let nodas () =
    let db = TzdbDateTimeZoneSource.Default
    let zone = db.ForId("America/Vancouver")
    let dt = LocalDateTime.FromDateTime(DateTime.Now)
    let zdt = dt.InZoneLeniently(zone)
    let utc = zdt.ToDateTimeUtc()
    printfn $"zone: {zone}"
    printfn $"DT: {dt}"
    printfn $"zdt: {zdt}"
    printfn $"utc: {utc}"
    printfn $"instant: {zdt.ToInstant()}"
    ()

let asJson = JsonSerializer.Serialize(tzs)

let writeToFile () =
    File.WriteAllText("./tzs.json", asJson)

nodas()