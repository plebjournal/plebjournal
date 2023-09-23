module Stacker.Web.Timezone

open System
open NodaTime
open NodaTime.TimeZones

let private db = TzdbDateTimeZoneSource.Default

let defaultTimeZone = db.ForId("America/Vancouver")

let parseTimezone (zone: string) =
    let zone = db.ForId(zone)
    if zone <> null then zone else defaultTimeZone

let treatAsUserTimeZone (preferredUserZone: DateTimeZone) (dateTime: DateTime) =
    let dt = LocalDateTime.FromDateTime(dateTime)
    dt.InZoneLeniently(preferredUserZone)
    
let fromUtcToZone (preferredUserZone: DateTimeZone) (dateTime: DateTime) =
    let dt = Instant.FromDateTimeUtc(dateTime)
    dt.InZone(preferredUserZone)
    
let currentTimeIn (zone: DateTimeZone) =
    let now = Instant.FromDateTimeUtc(DateTime.UtcNow)
    now.InZone(zone)
    
let allZoneIds () =
    db.ZoneLocations
    |> Seq.map (fun z -> z.ZoneId)
    |> Seq.toList