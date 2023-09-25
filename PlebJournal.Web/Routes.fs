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
            return! redirectTo false "/login" next ctx
        }

let secureRoutes = withAuth >=> router {
    // Pages
    get "/dashboard" Handlers.Pages.dashboard
    get "/transactions" Handlers.Pages.transactions
    get "/notes" Handlers.Pages.notes
    get "/blockchaininfo" Handlers.Pages.blockChainInfo
    get "/indicators" Handlers.Pages.indicators
    get "/work-bench" Handlers.Pages.workbench
    get "/dca-calculator" Handlers.Pages.dcaCalculator
    get "/settings" Handlers.Pages.settings
    
    // User Settings
    get "/settings/user-settings" (withUserId Handlers.Partials.userSettings)
    post "/settings/user-settings" (withUserId Handlers.Form.updateSettings)
    
    // Transactions
    get "/bought" Handlers.Partials.boughtBitcoinForm
    getf "/tx/edit/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Partials.editForm(txId, userId)))
    putf "/tx/edit/%O" (fun (txId: Guid) -> withUserId(fun userId -> Handlers.Form.editTx(txId, userId)))
    getf "/tx/details/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Partials.txDetails(txId, userId)))
    getf "/tx/delete/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Partials.deleteForm (txId, userId)))
    deletef "/tx/delete/%O" (fun (txId: Guid) -> withUserId (fun userId -> Handlers.Form.deleteTx (txId, userId)))
    get "/tx-successful-toast" Handlers.Partials.txSuccessfulToast
    post "/bought" (withUserId Handlers.Form.createTx)
    
    // Notes
    get "/take-a-note" (withUserId Handlers.Partials.notesForm)
    post "/note" (withUserId Handlers.Form.createNote)
    get "/notes-list" (withUserId Handlers.Partials.listNotes)
    getf "/notes/%O" (fun (noteId: Guid) -> withUserId (fun userId -> Handlers.Form.noteDetails(noteId, userId)))
        
    // DCA Calculator
    post "/dca-calculator" (Handlers.Form.dcaCalculation)
    
    // Workbench
    post "/workbench/formula" Handlers.Form.formula
    get "/workbench/formula-designer" Handlers.Partials.workbenchFormulaDesigner
    post "/workbench/formula/sma" Handlers.Partials.workbenchFormulaSma

    // Import CSV
    get "/import" Handlers.Partials.importForm
    post "/import" (withUserId Handlers.Form.upload)
    
    // Blockchain Info
    get "/epochs" Handlers.Partials.epochs
    get "/history" (withUserId Handlers.Partials.history)
    
    // Widgets
    get "/balance" (withUserId Handlers.Partials.balance)
    get "/fiat-value" (withUserId Handlers.Partials.fiatValue)
    get "/btc-price" (withUserId Handlers.Partials.btcPrice)
    
    // Chart Containers
    get "/chart" Handlers.Partials.chart
    get "/workbench-chart" Handlers.Partials.workbenchChart
    get "/wma-chart" Handlers.Partials.``200 wma``
    
    get "/charts/fiat-value" Handlers.Partials.fiatValueChart
    get "/charts/dca-calculator" Handlers.Partials.dcaCalculatorChart
    
    // APIs
    get "/api/portfolio-summary" (withUserId Handlers.Api.portfolioSummary)
    get "/api/200-wma" (withUserId Handlers.Api.``200 wma api``)
    get "/api/workbench-config" Handlers.Api.workbenchConfig
    get "/api/fiat-value-chart-config" (withUserId Handlers.Api.fiatValueChartConfig)
    get "/api/dca-calculator" (withUserId Handlers.Api.dcaCalculatorChartConfig)
    get "/api/btc-price-chart" (withUserId Handlers.Api.btcPriceChart)
}

let publicRoutes = router {
    get "/" Handlers.Pages.index
    get "/logout" logout
    get "/create-account" Handlers.Pages.createAccount
    post "/create-account" Handlers.Form.createAccount
    get "/login" Handlers.Pages.login
    get "/login/lnauth" Handlers.Pages.lnAuth
    get "/login/lnauth/qrcode" Handlers.Partials.lnAuthQrCode
    get "/login/lnauth/callback" Handlers.Form.lnAuthCallback
    get "/login/lnauth/check" Handlers.Form.lnAuthCheck
    post "/login" Handlers.Form.login
    get "/nav/user" Handlers.Partials.userNav
}

let topRouter : HttpHandler =
    choose [
       publicRoutes
       secureRoutes
    ]
