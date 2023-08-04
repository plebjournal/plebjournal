namespace Stacker.Web.Jobs

open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Identity
open PlebJournal.Db
open Quartz

module CreateUser =
    let adminExists (userManager: UserManager<PlebUser>) =
        task {
            let! admin = userManager.FindByNameAsync("plebjournal")
            return admin <> null
        }

    let createAdminUser (userManager: UserManager<PlebUser>) =
        let adminUser = PlebUser("plebjournal")
        task {
            let! res = userManager.CreateAsync(adminUser, "Password123")
            let! b = userManager.AddToRoleAsync(adminUser, "admin")
            return ()
        }

    let adminRoleExists (roleManager: RoleManager<Role>) =
        roleManager.RoleExistsAsync("admin")

    let createAdminRole (roleManager: RoleManager<Role>) =
        task {
            let! exists = adminRoleExists roleManager

            if exists then
                ()
            else
                let adminRole = Role()
                adminRole.Name <- "admin"
                let! _ = roleManager.CreateAsync(adminRole)
                return ()
        }

type CreateUserJob
    (
        loggerFactory: ILoggerFactory,
        userManager: UserManager<PlebUser>,
        roleManager: RoleManager<Role>
    ) =
    interface IJob with
        member this.Execute _ =
            let logger = loggerFactory.CreateLogger("MyBackgroundJob")

            logger.LogInformation("Creating Admin User")

            task {
                let! adminExists = CreateUser.adminExists userManager

                if adminExists then
                    return ()
                else
                    do! CreateUser.createAdminRole roleManager
                    do! CreateUser.createAdminUser userManager

                return ()
            }
