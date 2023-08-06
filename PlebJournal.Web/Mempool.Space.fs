module Stacker.Web.Mempool_Space

open FsHttp

let url = "https://mempool.space/api/blocks/tip/height"

let getBlockchainTip () = task {
    let! resp = http { GET url } |> Request.sendAsync
    let! body = resp.content.ReadAsStringAsync()
    return int body
}