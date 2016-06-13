using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MyThings.Common.Models;
using MyThings.Common.Repositories;

namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Context.MyThingsContext>
    {
        //Define the repositories
        private readonly SensorRepository _sensorRepository = new SensorRepository();
        private readonly ContainerRepository _containerRepository = new ContainerRepository();
        private readonly ContainerTypeRepository _containerTypeRepository = new ContainerTypeRepository();
        private readonly GroupRepository _groupRepository = new GroupRepository();
        private readonly ErrorRepository _errorRepository = new ErrorRepository();
        private readonly PinRepository _pinRepository = new PinRepository();

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

            //Generate dummy data for the database
            GenerateDummyData(appUser);
        }

        #region DummyDataGenerator
        private void GenerateDummyData(ApplicationUser user)
        {
            //Make a new dummy sensor
            Sensor dummySensor = new Sensor()
            {
                Name = "Lora Mc Loraface",
                Company = "Telenet",
                MACAddress = Guid.NewGuid().ToString(),
                Location = "Hier",
                CreationDate = DateTime.Now,
                SensorEntries = 1,
                Lat = 50.8245485,
                Lng = 3.2500197,
                Accuracy = 1
            };
            _sensorRepository.Insert(dummySensor);
            _sensorRepository.SaveChanges();

            //Make a dummy containertype
            ContainerType type = new ContainerType() { Name = "Drughs Container" };
            _containerTypeRepository.Insert(type);
            _containerTypeRepository.SaveChanges();

            //Make a dummy container
            Container dummyContainer = new Container()
            {
                Name = "Frank",
                MACAddress = dummySensor.MACAddress,
                CreationTime = DateTime.Now,
                ContainerType = type,
                SensorId = dummySensor.Id
            };
            _containerRepository.Insert(dummyContainer);
            _containerRepository.SaveChanges();

            //Add container to sensor
            dummySensor.Containers = new List<Container>() { dummyContainer };
            _sensorRepository.Update(dummySensor);
            _sensorRepository.SaveChanges();

            //Make a dummy group
            Group dummyGroup = new Group()
            {
                Name = "Plankton",
                Sensors = new List<Sensor>()
                {
                    dummySensor
                }
            };
            _groupRepository.Insert(dummyGroup);
            _groupRepository.SaveChanges();

            //Make a dummy error
            Error dummyWarning = Error.GenericWarning(dummySensor, dummyContainer, "");
            Error dummyError = Error.GenericError(dummySensor, dummyContainer, "");
            _errorRepository.Insert(dummyError);
            _errorRepository.Insert(dummyWarning);
            _errorRepository.SaveChanges();

            //Make a dummy pin
            Pin dummyPin = new Pin()
            {
                UserId = user.Id,
                SavedId = dummySensor.Id,
                SavedType = PinType.Sensor
            };
            _pinRepository.Insert(dummyPin);

            //Make a second dummy pin
            Pin dummyPin2 = new Pin()
            {
                UserId = user.Id,
                SavedId = dummyGroup.Id,
                SavedType = PinType.Group
            };
            _pinRepository.Insert(dummyPin2);
            _pinRepository.SaveChanges();
        }

        #endregion
    }
}
