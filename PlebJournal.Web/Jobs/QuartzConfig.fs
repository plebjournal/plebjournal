module Stacker.Web.Jobs.QuartzConfig


open System
open Quartz
open Stacker.Web.Jobs.CurrentPrice
open Stacker.Web.Jobs.InitializePrices

let addJob<'t when 't :> IJob> (q: IServiceCollectionQuartzConfigurator) (id: string) =
    q.AddJob<'t>(fun job -> job.WithIdentity(id) |> ignore) |> ignore

let configure (quartz: IServiceCollectionQuartzConfigurator) =
    quartz.UseMicrosoftDependencyInjectionJobFactory()

    // Create an admin user
    quartz.AddJob<CreateUserJob>(fun job -> job.WithIdentity("CreateUserJob") |> ignore)
    |> ignore

    quartz.AddTrigger(fun trigger -> trigger.ForJob("CreateUserJob").StartNow() |> ignore)
    |> ignore

    // Update historical prices
    quartz.AddJob<UpdateHistorical>(fun job -> job.WithIdentity("UpdateHistoricalPricesJob") |> ignore)
    |> ignore

    quartz.AddTrigger(fun trigger ->
        trigger
            .ForJob("UpdateHistoricalPricesJob")
            .StartAt(DateTimeOffset.Now.AddSeconds(5))
            .WithSimpleSchedule(fun sched -> sched.RepeatForever().WithIntervalInHours(12) |> ignore)
        |> ignore)
    |> ignore

    // Update Current Price
    quartz.AddJob<CurrentPrice>(fun job -> job.WithIdentity("UpdateCurrentPrices") |> ignore)
    |> ignore

    quartz.AddTrigger(fun trigger ->
        trigger
            .ForJob("UpdateCurrentPrices")
            .StartNow()
            .WithSimpleSchedule(fun sched -> sched.RepeatForever().WithIntervalInMinutes(60) |> ignore)
        |> ignore)
    |> ignore

    ()
