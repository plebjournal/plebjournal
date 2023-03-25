namespace Stacker.Web.Jobs

open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Identity
open Quartz

module CreateUser =
    let adminExists (userManager: UserManager<IdentityUser>) =
        task {
            let! admin = userManager.FindByNameAsync("admin")
            return admin <> null
        }

    let createAdminUser (userManager: UserManager<IdentityUser>) =
        let adminUser = IdentityUser("jsaucier")

        task {
            let! res = userManager.CreateAsync(adminUser, "P@ssw0rd!")
            let! b = userManager.AddToRoleAsync(adminUser, "admin")
            return ()
        }

    let adminRoleExists (roleManager: RoleManager<IdentityRole>) = roleManager.RoleExistsAsync("admin")

    let createAdminRole (roleManager: RoleManager<IdentityRole>) =
        task {
            let! exists = adminRoleExists roleManager

            if exists then
                ()
            else
                let adminRole = IdentityRole("admin")
                let! _ = roleManager.CreateAsync(adminRole)
                return ()
        }

type CreateUserJob
    (
        loggerFactory: ILoggerFactory,
        userManager: UserManager<IdentityUser>,
        roleManager: RoleManager<IdentityRole>
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
