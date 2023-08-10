module Stacker.Web.Handlers

open System
open System.IO
open System.Security.Claims
open FParsec.CharParsers
open Giraffe
open Giraffe.Htmx
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open PlebJournal.Db
open Stacker
open GenerateSeries
open Domain
open Stacker.Web
open Models
open FsToolkit.ErrorHandling
open Stacker.Web.Models
open Stacker.Web.Views
open Stacker.Web.Views.Pages
open Repository

module Pages =
    let index: HttpHandler = Index.indexPage |> Layout.withLayout |> htmlView

    let transactions: HttpHandler =
        Transactions.transactionsPage |> Layout.withLayout |> htmlView
        
    let blockChainInfo: HttpHandler =
        BlockchainInfo.blockchainInfoPage |> Layout.withLayout |> htmlView

    let indicators: HttpHandler =
        Indicators.indicatorsPage |> Layout.withLayout |> htmlView

    let workbench: HttpHandler =
        Workbench.workbenchPage |> Layout.withLayout |> htmlView
    
    let login: HttpHandler =
        Login.loginPage |> Layout.withLayout |> htmlView
        
    let createAccount: HttpHandler =
        CreateAccount.createAccountPage |> Layout.withLayout |> htmlView
        
    let twitter: HttpHandler =
        Twitter.twitterPage |> Layout.withLayout |> htmlView
    
    let dcaCalculator: HttpHandler =
        DcaCalculator.dcaCalculatorPage |> Layout.withLayout |> htmlView
        
module Partials =
    let userNav: HttpHandler =
        fun next ctx ->
            let user =
                match ctx.User.Identity.IsAuthenticated with
                | false -> None
                | true ->
                    let claim = ctx.User.Claims |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Name)
                    claim |> Option.map (fun c -> c.Value)
            htmlView (Partials.User.userNav user) next ctx
    let boughtBitcoinForm: HttpHandler =
        withHxTriggerAfterSettle "open-modal" >=> htmlView Partials.Forms.boughtBtcModal
        
    let txDetails (txId: Guid, userId: Guid) : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            let users = ctx.GetService<UserManager<PlebUser>>()
            task {
                let! tx = Transactions.Read.getTxById db userId txId
                let! price = CurrentPrice.Read.getCurrentPrice db CAD
                
                return!
                    match tx with
                    | None -> RequestErrors.NOT_FOUND "not found" next ctx
                    | Some t ->
                        let change = Calculate.percentChange price t
                        htmlView (Partials.Forms.txDetails t change) next ctx
            }
        
    let deleteForm (txId: Guid, userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! tx = Transactions.Read.getTxById db userId txId
                
                let resp =
                    match tx with
                    | None -> RequestErrors.NOT_FOUND "tx not found" 
                    | Some t ->  htmlView (Partials.Forms.deleteModal t)
                    
                return! resp next ctx
            }
            
    let editForm (txId: Guid, userId: Guid) : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! t = Transactions.Read.getTxById db userId txId
                
                let resp =
                    match t with
                    | None -> RequestErrors.NOT_FOUND "Tx not found"
                    | Some tx -> htmlView (Partials.Forms.editModal tx)
                
                return! resp next ctx
            }
        
    let txSuccessfulToast: HttpHandler = htmlView (Partials.Toast.txToast ())
    let importForm: HttpHandler = htmlView (Partials.Forms.importForm [])

    let history (userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>() 
            let dateHorizon (horizon: TxHistoryHorizon option) =
                let now = DateTime.UtcNow
                match horizon with
                | Some TwoMonths -> now.AddMonths(-2) |> Some
                | Some TwelveMonths -> now.AddMonths(-12) |> Some
                | Some TwoYears -> now.AddMonths(-24) |> Some
                | Some AllData -> None
                | _ -> None
                
            let fetchTxs (db: PlebJournalDb) (userId: Guid) (horizon: DateTime option) =
                match horizon with
                | Some d -> Transactions.Read.getTxsForUserInHorizon db userId d
                | None -> Transactions.Read.getAllTxsForUser db userId
            
            task {
                let horizon =
                    ctx.TryGetQueryStringValue "horizon"
                    |> Option.defaultValue "12-months"
                    |> TxHistoryHorizon.parse
                
                let! txs = fetchTxs db userId (dateHorizon horizon)
                let! currentPrice = CurrentPrice.Read.getCurrentPrice db CAD
                    
                let txHistoryModel =
                    txs
                    |> Array.map (fun tx ->
                        { Transaction = tx
                          PercentChange = Calculate.percentChange currentPrice tx
                          Ngu = Calculate.txNgu currentPrice tx })
                    |> Array.toList
                    
                return! htmlView (Partials.TxHistory.historyTable txHistoryModel horizon) next ctx
            }
    let epochs : HttpHandler =
        fun next ctx -> task {
            let! currentBlockHeight = Mempool_Space.getBlockchainTip ()
            return! htmlView (Partials.Epochs.epochChart currentBlockHeight) next ctx
        }

    let balance (userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! cadPrice = CurrentPrice.Read.getCurrentPrice db CAD
                let! txs = Transactions.Read.getAllTxsForUser db userId
                let sixMonthsAgo = DateTime.Now.AddMonths(-6).Date;
                let txsUntil6MonthsAgo =
                    txs
                    |> Array.where (fun t -> t.DateTime < sixMonthsAgo)
                let totalStack6MonthsAgo =
                    txsUntil6MonthsAgo |> Calculate.foldTxs |> (fun btc -> { Total = btc })
                let totalStackToday = txs |> Calculate.foldTxs |> (fun btc -> { Total = btc })
                                
                let value =
                    totalStackToday.Total * cadPrice
                    |> decimal
                    |> Calculate.twoDecimals
                    |> (*) 1.0m<btc>
                    
                let change = Calculate.numericalChange
                                 (decimal totalStack6MonthsAgo.Total)
                                 (decimal totalStackToday.Total)
                                                     
                return!
                    htmlView (Partials.Widgets.btcBalance totalStackToday (Some value) change) next ctx
            }
    let fiatValue (userId: Guid) : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! cadPrice = CurrentPrice.Read.getCurrentPrice db CAD
                let! txs = Transactions.Read.getAllTxsForUser db userId
                let res = txs |> Calculate.foldTxs |> (fun btc -> { Total = btc })
                
                let totalValueToday = res.Total * cadPrice
                let costBasis = Calculate.fiatCostBasis txs
                let vm = {
                    CostBasis = costBasis
                    Balance = res
                    CurrentValue = totalValueToday
                    Ngu = Calculate.ngu (decimal totalValueToday) costBasis
                }
                return! htmlView (Partials.Widgets.fiatValue vm) next ctx
            }
            
    let btcPrice : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! cadPrice = CurrentPrice.Read.getCurrentPrice db CAD
                let! usdPrice = CurrentPrice.Read.getCurrentPrice db USD
                
                return! htmlView (Partials.Widgets.btcPrice (cadPrice, usdPrice)) next ctx
            }
    
    let chart: HttpHandler =
        fun next ctx ->
            let horizon =
                ctx.TryGetQueryStringValue "horizon"
                |> Option.defaultValue "12-months"            
            
            let triggerChart = withHxTriggerManyAfterSettle [
                "show-chart", horizon
            ]
            let txHorizon = TxHistoryHorizon.parse horizon
            let handler = triggerChart >=> htmlView (Partials.Charts.chartContainer txHorizon)
            handler next ctx
        
    let fiatValueChart: HttpHandler =
        fun next ctx ->
            let horizon =
                ctx.TryGetQueryStringValue "horizon"
                |> Option.defaultValue "12-months"            
            
            let triggerChart = withHxTriggerManyAfterSettle [
                "show-chart-fiat-value", horizon
            ]
            let txHorizon = TxHistoryHorizon.parse horizon
            let handler = triggerChart >=> htmlView (Partials.Charts.fiatChartContainer txHorizon)            
            handler next ctx
            
    let workbenchChart: HttpHandler = htmlView (Partials.Charts.workbenchChartContainer)
    let workbenchFormulaDesigner : HttpHandler = htmlView (Partials.Forms.workbenchFormulaDesigner None Repository.WorkbenchChart.workbenchCharts)
    let workbenchFormulaSma : HttpHandler = htmlView (Partials.Forms.workbenchFormulaDesigner (Some "sma(btc-usd, 7)") [])
    let ``200 wma``: HttpHandler = htmlView Partials.Charts.wmaChartContainer
    let dcaCalculatorChart : HttpHandler = htmlView (Partials.Charts.dcaCalculatorChartContainer Repository.DcaCalculation.dcaCalculation)

module Form =
    
    let createAccount: HttpHandler =
        fun next ctx ->
            let logger = ctx.GetLogger("CreateAccount")
            let userManager = ctx.GetService<UserManager<PlebUser>>()
            let signInManager = ctx.GetService<SignInManager<PlebUser>>()
            
            let validate (create: CreateNewAccount) =
                if create.Password <> create.PasswordRepeat then
                    Error { Username = None; Password = Some "Passwords should match"; Identity = [] }
                else if String.IsNullOrWhiteSpace(create.Username) then
                    Error { Username = Some "Username cannot be empty"; Password = None; Identity = [] } else
                Ok create
            
            let success = withHxRedirect "/" >=> htmlView (CreateAccount.createAccountForm None)
            let backToForm err = setStatusCode 422 >=> htmlView (CreateAccount.createAccountForm (Some err))
            task {
                let! newAccount = ctx.BindFormAsync<CreateNewAccount>()
                let valid = validate newAccount
                match valid with
                | Ok account ->
                    let identity = PlebUser(account.Username)
                    
                    let! user = userManager.CreateAsync(identity, account.Password)
                    if user.Succeeded then
                        do! signInManager.SignInAsync(identity, false)
                        return! success next ctx
                    else
                        let e = user.Errors |> Seq.toList |> List.map (fun e -> e.Description)
                        logger.LogInformation("Failed to create new user", e)
                        let errViewModel =
                            { Username = None
                              Password = None
                              Identity = e }
                        return! (backToForm errViewModel) next ctx 
                | Error err ->
                    return! (backToForm err) next ctx
            }
            
    let login: HttpHandler =
        fun next ctx ->
            let logger = ctx.GetLogger("Login")
            let signInManager = ctx.GetService<SignInManager<PlebUser>>()
            let backToLoginForm msg = setStatusCode 422 >=> htmlView (Login.loginForm (Some msg)) 
            task {
                let! loginForm = ctx.TryBindFormAsync<Login>()
                match loginForm with
                | Error e ->
                    logger.LogInformation("Failed bind signin form {err}", e)
                    return! backToLoginForm "Error, please try again" next ctx
                | Ok form ->
                    let! res = signInManager.PasswordSignInAsync(
                        form.Username,
                        form.Password,
                        false,
                        false
                    )
                    logger.LogInformation("Login result {res}", res)
                    match res.Succeeded with
                    | true ->
                        let a = withHxRedirect "/" >=> htmlView (Login.loginForm None)
                        return! a next ctx
                    | false -> 
                        return! backToLoginForm "Invalid username/password" next ctx
            }
    
    let upload (userId: Guid) : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let files = ctx.Request.Form.Files
                let file = files |> Seq.head
                
                let stream = file.OpenReadStream()
                let reader = new StreamReader(stream)
                let! a = reader.ReadToEndAsync()
                
                let res = Import.import a
                let txs =
                    res
                    |> List.collect (fun t -> match t with | Ok tx -> [ tx ] | Error _ -> [])
                
                let errs =
                    res
                    |> List.collect
                           (fun t ->
                            match t with
                            | Ok tx -> []
                            | Error errorValue -> errorValue)
                
                do! Transactions.Insert.insertMany db txs userId
                let withTriggers = withHxTriggerManyAfterSettle [
                    "tx-created", ""
                    "showMessage", $"Imported {txs.Length} transactions"
                ]
                return! (withTriggers >=> htmlView (Views.Partials.Forms.importForm errs)) next ctx
        }
    
    let createTx (userId: Guid): HttpHandler =
        let createBtcAmount (createTx: CreateBtcTransaction) =
            match createTx.BtcUnit with
            | Btc -> Domain.Btc(createTx.BtcAmount * 1.0m<btc>)
            | Sats -> Domain.Sats(int64 createTx.BtcAmount * 1L<sats>)
            
        let toDomain (createTx: CreateBtcTransaction) =
            let withOffset = createTx.Date.AddMinutes(createTx.TimeZoneOffset)
            let txDate = DateTime.SpecifyKind(withOffset, DateTimeKind.Utc)
            match createTx.Type with
            | Buy ->
                Domain.Buy
                    { Id = Guid.NewGuid()
                      Date = txDate
                      Amount = createBtcAmount createTx
                      Fiat = { Amount = createTx.FiatAmount; Currency = createTx.Fiat } }
            | Income ->
                Domain.Income
                    { Id = Guid.NewGuid()
                      Date = txDate
                      Amount = createBtcAmount createTx }
            | Sell ->
                Domain.Sell
                    { Id = Guid.NewGuid()
                      Date = txDate
                      Amount = createBtcAmount createTx
                      Fiat = { Amount = createTx.FiatAmount; Currency = createTx.Fiat } }
            | Spend ->
                Domain.Spend
                    { Id = Guid.NewGuid()
                      Date = txDate
                      Amount = createBtcAmount createTx }
        
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! boughtBtc = ctx.BindFormAsync<CreateBtcTransaction>()
                let validated = Validation.validateNewTransaction boughtBtc
                match validated with
                | Ok tx ->
                    let domain = toDomain tx
                    do! Transactions.Insert.insertTx db domain userId
                    let withTriggers = withHxTriggerManyAfterSettle [
                        "tx-created", ""
                        "showMessage", Alerts.alert domain
                    ]
                    return!
                        (withTriggers >=> htmlView (Views.Partials.Forms.newTxsForm [])) next ctx
                | Error errs ->
                    return! (htmlView (Views.Partials.Forms.newTxsForm errs)) next ctx
            }
                    
    let formula: HttpHandler =
        fun next ctx ->
            let log = ctx.GetLogger("Formula")
            task {
                let! formula = ctx.BindFormAsync<Formula>()
                let f = formula.Formula
                let a = Charting.Parsing.parse f
                
                let a = 
                    match a with
                    | Success (graphableDataSeries, _, _) ->
                        let appended = (formula.FormulaName, graphableDataSeries) :: Repository.WorkbenchChart.workbenchCharts
                        Repository.WorkbenchChart.workbenchCharts <- appended
                        log.LogInformation("graphable series {graphableDataSeries}", graphableDataSeries)
                        
                        let view = Views.Partials.Forms.workbenchFormulaDesigner (Some f) Repository.WorkbenchChart.workbenchCharts
                        (withHxTrigger "formula-updated" >=> htmlView view) next ctx
                    | Failure(s, parserError, unit) ->
                        log.LogError("failed to parse {s}", s)                 
                        (htmlView (Views.Partials.Forms.workbenchFormulaDesigner (Some f) Repository.WorkbenchChart.workbenchCharts)) next ctx
                return! a
            }
            
    let dcaCalculation : HttpHandler =
        fun next ctx ->
            task {
                let! form = ctx.BindFormAsync<DcaCalculation>()
                let req =
                    { GenerateRequest.Start = form.StartDate
                      Cadence = form.Cadence
                      Duration = form.Duration, form.DurationUnit
                      FiatAmount = form.Amount }
                Repository.DcaCalculation.dcaCalculation <- req
                
                return! (withHxTrigger "dca-calculated" >=> (htmlView (Partials.Forms.dcaCalculatorForm req))) next ctx
            }
    
    let deleteTx (txId: Guid, userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                do! Transactions.Delete.deleteTxForUser db txId userId
                let trigger = withHxTrigger "tx-deleted"
                return!
                    (trigger >=> Successful.OK ()) next ctx
            }
            
    let editTx (txId: Guid, userId: Guid): HttpHandler =
        let createBtcAmount (editTx: EditBtcTransaction) =
            match editTx.BtcUnit with
            | Btc -> Domain.Btc(editTx.Amount * 1.0m<btc>)
            | Sats -> Domain.Sats(int64 editTx.Amount * 1L<sats>)

        let toDomain (editTx: EditBtcTransaction) =
            let withOffset = editTx.Date.AddMinutes(editTx.TimeZoneOffset)
            let txDate = DateTime.SpecifyKind(withOffset, DateTimeKind.Utc)

            match editTx.Type with
            | Buy ->
                Domain.Buy
                    { Id = txId
                      Date = txDate
                      Amount = createBtcAmount editTx
                      Fiat = { Amount = editTx.FiatAmount; Currency = editTx.Fiat } }
            | Income ->
                Domain.Income
                    { Id = txId
                      Date = txDate
                      Amount = createBtcAmount editTx }
            | Sell ->
                Domain.Sell
                    { Id = txId
                      Date = txDate
                      Amount = createBtcAmount editTx
                      Fiat = { Amount = editTx.FiatAmount; Currency = editTx.Fiat } }
            | Spend ->
                Domain.Spend
                    { Id = txId
                      Date = txDate
                      Amount = createBtcAmount editTx }
        
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! update = ctx.BindFormAsync<EditBtcTransaction>()
                let validated = Validation.validateEditedTransaction update
                match validated with
                | Error e ->
                    let! t = Transactions.Read.getTxById db userId txId
                    return! (htmlView (Views.Partials.Forms.editTxForm t.Value e)) next ctx
                | Ok tx ->
                    let domain = toDomain tx
                    let! changed = Transactions.Update.updateTx db domain userId
                    let res =
                        match changed with
                        | None -> RequestErrors.NOT_FOUND "TX Not found" next ctx
                        | Some () ->
                            let withTriggers = withHxTriggerManyAfterSettle [
                                "tx-updated", ""
                                "showMessage", "Transaction Updated"
                            ]
                            (withTriggers >=> Successful.OK ()) next ctx
                    
                    return! res
            }
            
module Api =
    let chartApi (userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! txs = Transactions.Read.getAllTxsForUser db userId

                let model =
                    txs
                    |> Calculate.foldDailyTransactions
                    |> Calculate.movingSumOfTxs
                    |> Seq.toList
                    
                let append =
                    match List.tryLast model with
                    | Some (d, amount) ->
                        [ DateTime.Today.Date, amount ]
                    | None -> []
                    
                let appended = model @ append

                let trace = {|
                    name = "BTC Stack"
                    mode = "lines"
                    x = appended |> Seq.map fst
                    y = appended |> Seq.map snd
                    fill = "tozeroy"
                |}
                
                let config = {|
                    traces = [ trace ]
                |}
                return! json config next ctx
            }

    let ``200 wma api`` (userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! prices = Prices.Read.getPrices db USD
                                   
                let wma = Calculate.ma prices 1400

                let priceSeries =
                    prices
                    |> Seq.map (fun wma -> {| x = wma.Date; y = wma.Price |}) |> Seq.toArray
                    |> Seq.filter (fun xy -> xy.x > DateTime(2014, 01, 01))

                let wmaSeries =
                    wma
                    |> Seq.map (fun wma -> {| x = wma.Date; y = wma.MA |})
                    |> Seq.toArray
                
                let! txs = Transactions.Read.getAllTxsForUser db userId
                let purchases =
                    txs
                    |> Array.filter (fun tx -> tx.PricePerCoin.IsSome)
                    |> Array.map (fun tx ->
                        {| x = tx.DateTime; y = tx.PricePerCoin.Value |> fst |})

                return! json
                    {| price = priceSeries
                       wma = wmaSeries
                       purchases = purchases |}
                    next
                    ctx
            }

    let portfolioValue (userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! txs = Transactions.Read.getAllTxsForUser db userId
                let! prices = Prices.Read.getPrices db CAD
                let res = Calculate.portfolioHistoricalValue txs prices

                let viewModel =
                    res
                    |> List.map (fun (p, amount, value) ->
                        {| date = p.Date
                           btcPrice = p.Price
                           amount = amount
                           value = value |})
                    |> List.sortBy (fun xy -> xy.date)

                return! json viewModel next ctx
            }
            
    let portfolioSummary (userId: Guid) : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            let dateHorizon (horizon: TxHistoryHorizon option) =
                let now = DateTime.Now
                match horizon with
                | Some TwoMonths -> now.AddMonths(-2) |> Some
                | Some TwelveMonths -> now.AddMonths(-12) |> Some
                | Some TwoYears -> now.AddMonths(-24) |> Some
                | Some AllData -> None
                | _ -> None
                
            let horizon =
                ctx.TryGetQueryStringValue "horizon"
                |> Option.defaultValue "12-months"
                |> TxHistoryHorizon.parse
                |> dateHorizon
                |> Option.defaultValue (DateTime(2000, 01, 01))
                
            task {
                
                let! txs = Transactions.Read.getAllTxsForUser db userId
                let! pricesUsd = Prices.Read.getPrices db CAD
                
                let fiatValue =
                    Calculate.portfolioHistoricalValue txs pricesUsd
                    |> List.sortBy (fun (a, _,_) -> a.Date)
                    |> List.where (fun (d, _, _) -> d.Date >= horizon)
                
                let btcStack =
                    txs
                    |> Calculate.foldDailyTransactions
                    |> Calculate.movingSumOfTxs
                    |> Calculate.fillDatesWhichHaveNoTx
                    |> Array.where (fun (d, _) -> d >= horizon)
                
                let fiatTrace = Map<string, obj> [
                    "name", "Fiat Value"
                    "mode", "lines"
                    "x", fiatValue |> List.map (fun (d, _, _) -> d.Date) |> List.sort :> obj
                    "y", fiatValue |> List.map (fun (_, _, v) -> v) :> obj
                ]
                
                let btcStackTrace = Map<string, obj> [
                    "name", "Btc Value"
                    "mode", "lines"
                    "x", btcStack |> Array.map fst |> Array.toList :> obj
                    "y", btcStack |> Array.map snd |> Array.toList :> obj
                    "yaxis", "y2"
                ]
                
                let config = {| traces = [fiatTrace; btcStackTrace] |}
                
                return! json config next ctx
            }

    let workbenchConfig: HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! prices =
                    Prices.Read.getPrices db USD
                
                let chartWorkbench =
                    Repository.WorkbenchChart.workbenchCharts
                    |> List.map (fun (name, chart) ->
                        name, Charting.DataProcessing.evaluate prices chart)
                                    
                let usdTrace =
                    {| name = "Btc Price USD"
                       mode = "lines"
                       x = prices |> Seq.map (fun p -> p.Date)
                       y = prices |> Seq.map (fun p -> p.Price) |}
                       
                let customTraces =
                    chartWorkbench
                    |> List.map (fun (name, graph) -> {|
                        name = name
                        mode = "lines"
                        x = graph |> Seq.map (fun g -> g.X)
                        y = graph |> Seq.map (fun g -> g.Y)
                        |})
                    
                
                let config = {|
                    traces = usdTrace :: customTraces
                |}
                       
                return!
                    json config next ctx
            }
    
    let fiatValueChartConfig (userId: Guid) : HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let dateHorizon (horizon: TxHistoryHorizon option) =
                    let now = DateTime.Now
                    match horizon with
                    | Some TwoMonths -> now.AddMonths(-2) |> Some
                    | Some TwelveMonths -> now.AddMonths(-12) |> Some
                    | Some TwoYears -> now.AddMonths(-24) |> Some
                    | Some AllData -> None
                    | _ -> None
                    
                let horizon =
                    ctx.TryGetQueryStringValue "horizon"
                    |> Option.defaultValue "12-months"
                    |> TxHistoryHorizon.parse
                    |> dateHorizon
                    |> Option.defaultValue (DateTime(2000, 01, 01))
                
                let! txs = Transactions.Read.getAllTxsForUser db userId
                let! prices =
                    Prices.Read.getPrices db CAD
                    
                let res =
                    Calculate.portfolioHistoricalValue txs prices
                    |> List.filter (fun (d, _, _) -> d.Date >= horizon)
                
                let trace = {|
                    name = "Fiat Value"
                    mode = "lines"
                    x = res |> Seq.map (fun (d, _, _) -> d.Date)
                    y = res |> Seq.map (fun (_, _, v) -> v)
                    fill = "tozeroy"
                |}
            
                let config = {|
                    traces = [ trace ]
                |}
                
                return! json config next ctx
        }
            
    let dcaCalculatorChartConfig (userId: Guid): HttpHandler =
        fun next ctx ->
            let db = ctx.GetService<PlebJournalDb>()
            task {
                let! pricesUsd = Prices.Read.getPrices db USD
                    
                let txs =
                    generate (Array.toList pricesUsd) Repository.DcaCalculation.dcaCalculation
                    |> List.toArray
                
                let model =
                    txs
                    |> Calculate.foldDailyTransactions
                    |> Calculate.movingSumOfTxs
                    
                let fiatValueModel =
                    Calculate.portfolioHistoricalValue txs pricesUsd
                    |> List.sortBy (fun (p, _, _) -> p.Date)
                    
                let costBasis =
                    Calculate.movingCostBasis txs
                    |> List.sortBy fst
                
                let fiatTrace' = {|
                    name = "Fiat Value"
                    mode = "lines"
                    x = fiatValueModel |> List.map (fun (d, _, _) -> d.Date) |> List.sort
                    y = fiatValueModel |> List.map (fun (_, _, v) -> v)
                |}
                
                let btcStackTrace' = {|
                    name = "Btc Value"
                    mode = "lines"
                    x = model |> Seq.map fst |> Seq.toList
                    y = model |> Seq.map snd |> Seq.toList
                    yaxis = "y2"
                |}
                
                let costBasis = {|
                    name = "Cost Basis"
                    mode = "lines"
                    x = costBasis |> List.map fst
                    y = costBasis |> List.map snd
                |}
                                
                let config = {| traces = [ box fiatTrace'; btcStackTrace'; costBasis ] |}
                return! json config next ctx
            }