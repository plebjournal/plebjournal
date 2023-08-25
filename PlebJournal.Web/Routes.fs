module Stacker.Web.Routes

open System
open System.Security.Claims
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open PlebJournal.Db
open Saturn

let redirectToLogin = redirectTo false "/login"
let withAuth = requiresAuthentication redirectToLogin

let getUserId (ctx: HttpContext) =
    let user = ctx.User.Claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.NameIdentifier)
    user |> Option.map (fun claim -> Guid.Parse(claim.Value))

let withUserId (handler: Guid -> HttpHandler) : HttpHandler =
    fun next ctx ->
        match getUserId ctx with
        | Some id -> handler id next ctx
        | None -> redirectTo false "/login" next ctx
        
let logout : HttpHandler =
    fun next ctx ->
        task {
            let signInManager = ctx.GetService<SignInManager<PlebUser>>()
            do! signInManager.SignOutAsync()
            return! redirectTo false "/" next ctx
        }

let secureRoutes = withAuth >=> router {
    get "/" Handlers.Pages.index
    get "/transactions" Handlers.Pages.transactions
    get "/blockchaininfo" Handlers.Pages.blockChainInfo
    get "/indicators" Handlers.Pages.indicators
    get "/work-bench" Handlers.Pages.workbench
    get "/dca-calculator" Handlers.Pages.dcaCalculator
    get "/settings" Handlers.Pages.settings
    get "/settings/user-settings" (withUserId Handlers.Partials.userSettings)
    post "/settings/user-settings" (withUserId Handlers.Form.updateSettings)
    get "/bought" Handlers.Partials.boughtBitcoinForm
    getf "/tx/edit/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Partials.editForm(txId, userId)))
    putf "/tx/edit/%O" (fun (txId: Guid) -> withUserId(fun userId -> Handlers.Form.editTx(txId, userId)))
    getf "/tx/details/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Partials.txDetails(txId, userId)))
    getf "/tx/delete/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Partials.deleteForm (txId, userId)))
    deletef "/tx/delete/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Form.deleteTx (txId, userId)))
    get "/tx-successful-toast" Handlers.Partials.txSuccessfulToast
    post "/bought" (withUserId Handlers.Form.createTx)
    
    post "/dca-calculator" (Handlers.Form.dcaCalculation)
    post "/workbench/formula" Handlers.Form.formula
    get "/import" Handlers.Partials.importForm
    post "/import" (withUserId Handlers.Form.upload)
    get "/epochs" Handlers.Partials.epochs    
    get "/history" (withUserId Handlers.Partials.history)
    get "/balance" (withUserId Handlers.Partials.balance)
    get "/fiat-value" (withUserId Handlers.Partials.fiatValue)
    get "/btc-price" (withUserId Handlers.Partials.btcPrice)
    get "/chart" Handlers.Partials.chart
    get "/workbench/formula-designer" Handlers.Partials.workbenchFormulaDesigner
    post "/workbench/formula/sma" Handlers.Partials.workbenchFormulaSma
    get "/workbench-chart" Handlers.Partials.workbenchChart
    get "/wma-chart" Handlers.Partials.``200 wma``
    
    get "/charts/fiat-value" Handlers.Partials.fiatValueChart
    get "/charts/dca-calculator" Handlers.Partials.dcaCalculatorChart
    
    get "/api/portfolio-summary" (withUserId Handlers.Api.portfolioSummary)
    get "/api/200-wma" (withUserId Handlers.Api.``200 wma api``)
    
    get "/api/workbench-config" Handlers.Api.workbenchConfig
    get "/api/fiat-value-chart-config" (withUserId Handlers.Api.fiatValueChartConfig)
    get "/api/dca-calculator" (withUserId Handlers.Api.dcaCalculatorChartConfig)
    get "/api/btc-price-chart" (withUserId Handlers.Api.btcPriceChart)
}

let publicRoutes = router {
    get "/logout" logout
    get "/create-account" Handlers.Pages.createAccount
    post "/create-account" Handlers.Form.createAccount
    get "/login" Handlers.Pages.login
    post "/login" Handlers.Form.login
    get "/twitter" Handlers.Pages.twitter
    get "/nav/user" Handlers.Partials.userNav
    get "/razor/lib" Handlers.Pages.testing
    get "/razor/lib-2" Handlers.Pages.testing2
}

let topRouter : HttpHandler =
    choose [
       publicRoutes
       secureRoutes
    ]
