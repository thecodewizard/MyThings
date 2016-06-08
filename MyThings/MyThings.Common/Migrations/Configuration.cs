using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MyThings.Common.Models;

namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Context.MyThingsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Context.MyThingsContext context)
        {
            IdentityResult roleResult;
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!roleManager.RoleExists(ApplicationRoles.ADMIN))
                roleResult = roleManager.Create(new IdentityRole(ApplicationRoles.ADMIN));
            if (!roleManager.RoleExists(ApplicationRoles.USER))
                roleResult = roleManager.Create(new IdentityRole(ApplicationRoles.USER));

            if (!context.Users.Any(u => u.Email.Equals("lora@proximus.be")))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser()
                {
                    Email = "lora@proximus.be",
                    UserName = "lora@proximus.be",
                    EmailConfirmed = true
                };
                manager.Create(user, "LoRa1234!");
                manager.AddToRole(user.Id, ApplicationRoles.USER);
            }
        }
    }
}
