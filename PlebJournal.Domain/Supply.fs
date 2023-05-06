namespace Stacker

open Domain

module Supply =
    let rewardsSats =
        [| 0..32 |]
        |> Array.map (fun i -> (5_000_000_000L<sats>, i))
        |> Array.map (fun (reward, n) -> reward / (pown 2L n))

    let btcSupply (blockHeight: int) =
        if blockHeight < 0 then
            failwith "block height must be zero or greater"

        Array.zeroCreate<bool> (blockHeight + 1)
        |> Array.chunkBySize 210_000
        |> Array.map (fun arr -> arr.Length |> int64)
        |> Array.mapi (fun i blocks ->
            let reward = Array.tryItem i rewardsSats |> Option.defaultValue 0L<sats>
            blocks * reward)
        |> Array.sum
