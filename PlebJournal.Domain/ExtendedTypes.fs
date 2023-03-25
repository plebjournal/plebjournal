namespace Stacker 

module ExtendedTypes =

    module Array =
        let tryGetRange (startIdx: int) (endIdx: int) (arr: 'a array) =
            let maybeStart = Array.tryItem startIdx arr
            let maybeEnd = Array.tryItem endIdx arr
            match maybeStart, maybeEnd with
            | Some _, Some _ -> Some arr[startIdx .. endIdx]
            | _ -> None
            
    module Seq =
        let somes (xs: 'a option seq) = xs |> (Seq.choose id)
        
    module List =
        let somes (xs: 'a option list) = xs |> (List.choose id)
        let exceptLast (xs: 'a list) = xs[0 .. (xs.Length - 2)]
        