module Stacker.Web.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open PlebJournal.Db
open Quartz
open Saturn
open Stacker.Web
open Stacker.Web.Jobs
    
let addBackgroundJobs (svc: IServiceCollection) =
    svc.AddQuartz(QuartzConfig.configure)
        .AddQuartzServer(fun opts -> opts.WaitForJobsToComplete <- true)

let addIdentityDb (svc: IServiceCollection) =
    svc.AddDbContext<PlebJournalDb>()
        .AddIdentity<PlebUser, Role>()
        .AddEntityFrameworkStores() |> ignore
    svc.AddScoped<DbContext, PlebJournalDb>() |> ignore
    svc

let ensureDbExists (svc: IServiceProvider) =
    use scope = svc.CreateScope()
    let db = scope.ServiceProvider.GetRequiredService<PlebJournalDb>()
    let logger = svc.GetService<ILoggerFactory>().CreateLogger("DbMigration")
    logger.LogInformation("Attempting to create and migrate db")
    
    db.Database.Migrate()
    logger.LogInformation("Database migration complete")

let configureApp (app: IApplicationBuilder) =    
    do ensureDbExists app.ApplicationServices
    app.UseDeveloperExceptionPage() |> ignore
    app.UseAuthentication() |> ignore
    app

let app =
    application {
        use_static "wwwroot"
        use_router Routes.topRouter
        service_config addBackgroundJobs
        service_config addIdentityDb
        app_config configureApp
    }
   
run app
