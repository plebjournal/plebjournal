module Stacker.Web.Program

open System
open System.Reflection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Quartz
open Saturn
open PlebJournal.Identity
open Stacker.Web
open Stacker.Web.Config
open Stacker.Web.Jobs
    
let addBackgroundJobs (svc: IServiceCollection) =
    svc.AddQuartz(QuartzConfig.configure)
        .AddQuartzServer(fun opts -> opts.WaitForJobsToComplete <- true)

let addIdentityDb (svc: IServiceCollection) =
    svc.AddDbContext<StackerDbContext>(fun opts ->
            opts.UseNpgsql(connString()) |> ignore)
        .AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores() |> ignore
    svc.AddScoped<DbContext, StackerDbContext>() |> ignore
    svc

open DbUp

let ensureDbExists (svc: IServiceProvider) =
    let logger = svc.GetService<ILoggerFactory>().CreateLogger("DbUp")
    
    EnsureDatabase.For.PostgresqlDatabase(connString())
    
    let upgrade =
        DeployChanges.To.PostgresqlDatabase(connString())
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToAutodetectedLog()
            .Build()
            
    let res = upgrade.PerformUpgrade()
    
    match res.Successful with
    | true -> logger.LogInformation("Db Created Successfully")
    | false ->
        logger.LogCritical(``exception`` = res.Error, message = "Db upgrade failed")
        raise res.Error

let configureApp (app: IApplicationBuilder) =    
    do ensureDbExists app.ApplicationServices
    app.UseDeveloperExceptionPage() |> ignore
    app.UseAuthentication() |> ignore
    app

let app =
    application {
        use_static "wwwroot"
        use_router Routes.topRouter
        //service_config addBackgroundJobs
        service_config addIdentityDb
        app_config configureApp
    }
   
run app
