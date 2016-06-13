using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MyThings.Common.Models;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace MyThings.Common.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Context.MyThingsContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Context.MyThingsContext context)
        {
            //Make the ASP Roles
            IdentityResult roleResult;
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            if (!roleManager.RoleExists(ApplicationRoles.ADMIN))
                roleResult = roleManager.Create(new IdentityRole(ApplicationRoles.ADMIN));
            if (!roleManager.RoleExists(ApplicationRoles.USER))
                roleResult = roleManager.Create(new IdentityRole(ApplicationRoles.USER));

            //Make the lora@proximus.be user
            ApplicationUser appUser = new ApplicationUser();
            if (!context.Users.Any(u => u.Email.Equals("lora@proximus.be")))
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                var user = new ApplicationUser()
                {
                    Email = "lora@proximus.be",
                    UserName = "lora@proximus.be",
                    EmailConfirmed = true,
                    Company = "Proximus Demo"
                };
                manager.Create(user, "LoRa1234!");
                manager.AddToRole(user.Id, ApplicationRoles.USER);
                appUser = user;
            }
            else
            {
                var store = new UserStore<ApplicationUser>(context);
                var manager = new UserManager<ApplicationUser>(store);
                appUser = manager.FindByEmail("lora@proximus.be");
            }

            //Add a first timeholder object for the webjob registration
            Timeholder holder = new Timeholder();
            holder.WebjobInstanceStarted = DateTime.Now;
            holder.WebjobInstanceEnded = DateTime.Now.AddTicks(1);
            context.Timeholder.Add(holder);
        }    
    }
}
