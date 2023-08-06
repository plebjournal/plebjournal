namespace Stacker

open System
open Domain
open Stacker
open Supply

module Epochs =
    type Epoch =
        { EpochNumber: int
          TotalBlocks: int
          Reward: int64<sats>
          TotalSupply: int64<sats>
          Reached: bool }
        member this.RewardBtc =
            convertSatsToBtcInt64 this.Reward
            
        member this.RewardBinary =
            this.Reward
            |> int64
            |> fun reward -> Convert.ToString(reward, 2)
        
        member this.TotalSupplyPercent =
            this.TotalSupply
            |> convertSatsToBtcInt64
            |> fun total -> total / 21_000_000.0m
            |> fun d -> d * 100.0m
            |> decimal
            |> Calculate.twoDecimals

    let epochs (currentHeight: int) =
        [ 0 .. 32 ]
        |> List.map (fun i -> i * 210_000, i)
        |> List.map (fun (height, epoch) ->
            { EpochNumber = epoch
              TotalBlocks = height
              Reward = rewardsSats[epoch]
              TotalSupply = btcSupply height
              Reached = currentHeight > height })