module Stacker.Web.Db.Postgres

open Stacker.Web.Config
open Npgsql.FSharp

let conn = connString()

let postgresQuery query parameters mapper =
    conn
    |> Sql.connect
    |> Sql.query query
    |> Sql.parameters parameters
    |> Sql.executeAsync mapper
    
let postgresNonQuery query parameters =
    conn
    |> Sql.connect
    |> Sql.query query
    |> Sql.parameters parameters
    |> Sql.executeNonQueryAsync
    
let postgresManyNonQuery queries =
    conn
    |> Sql.connect
    |> Sql.executeTransactionAsync queries
