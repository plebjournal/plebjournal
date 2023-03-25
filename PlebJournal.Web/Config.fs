module Stacker.Web.Config

open System
open Microsoft.Extensions.Configuration

let envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")

let config =
    ConfigurationBuilder()
        .AddJsonFile("./appsettings.json")
        .AddJsonFile($"./appsettings.{envName}.json", optional = true)
        .AddEnvironmentVariables()
        .Build()

let connString () =
    let dbConfig = config.GetSection("Db")
    let userId = dbConfig["UserId"]
    let host = dbConfig["Host"]
    let password = dbConfig["Password"]
    let port = dbConfig["Port"]
    let db = dbConfig["Database"]
    $"User ID={userId};Password={password};Host={host};Port={port};Database={db};"
