namespace Stacker
module Alerts =

    open Stacker.Domain
    
    let buyMessages = [|
        "Come for the money, stay for the money"
        "LFG"
        "Stacking Sats"
        "[ stacking intensifies ]"
        "[ laser eyes intensify ]"
        "The stacking will continue until moral improves"
        "Do you even Bitcoin?"
    |]
    
    let incomeMessages = [|
        "Hyper-bitcoiner"
        "The Bitcoin economy grows"
        "'Bitcion can buy many peanuts'"
    |]
    
    let sellMessages = [|
        "Sometimes you need some fiat"
    |]
    
    let spendMessages = [|
        "The Bitcoin economy grows"
    |]
    
    
    let pickRandom (arr: string array) =
        let rand = System.Random()
        let next = rand.Next(0, arr.Length)
        arr[next]
        
    let alert (tx: Transaction) =
        match tx with
        | Buy _ -> pickRandom (Array.append buyMessages incomeMessages)
        | Income _ -> pickRandom (Array.append buyMessages incomeMessages)
        | Sell _ -> pickRandom sellMessages
        | Spend _ -> pickRandom spendMessages