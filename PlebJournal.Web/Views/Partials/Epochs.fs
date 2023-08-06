module Stacker.Web.Views.Partials.Epochs

open System
open Giraffe.ViewEngine
open Stacker

let epochChart (blockHeight: int) =
    let issuanceEpochs = Epochs.epochs blockHeight
    div [ _class "card" ] [
        div [ _class "table-responsive" ] [
            table [ _class "table card-table table-vcenter datatable" ] [
               thead [] [
                   tr [] [
                       th [] [ str "Epoch" ]
                       th [] [ str "Block Height" ]
                       th [] [ str "Reward (sats)" ]
                       th [] [ str "Reward (sats binary)" ]
                       th [] [ str "Total Supply (sats)" ]
                       th [] [ str "% of Total Supply" ]
                       th [] [ str "Reached" ]
                   ]
               ]
               tbody [] (issuanceEpochs |> List.map (fun e ->
                   tr [] [
                       td [] [ str $"{e.EpochNumber}" ]
                       td [] [ str $"{e.TotalBlocks}" ]
                       td [] [ str $"{e.Reward}" ]
                       td [] [ str $"{e.RewardBinary}" ]
                       td [] [ str $"{e.TotalSupply}" ]
                       td [] [ str $"{e.TotalSupplyPercent}" ]
                       td [] [ str (if e.Reached then ("✅") else "-") ]
                   ]
                ))
            ]
        ]
    ]
