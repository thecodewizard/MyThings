using System.Data.Entity;
using MyThings.Common.Models;

namespace MyThings.Common.Context
{
    public class MyThingsContext : ApplicationDbContext
    {
        //Basic Models
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Container> Container { get; set; }
        public DbSet<ContainerType> ContainerTypes { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Error> Error { get; set; }
        public DbSet<Timeholder> Timeholder { get; set; }
        public DbSet<Threshold> Threshold { get; set; }

        //Front-end Models
        public DbSet<Pin> Pins { get; set; }

        public MyThingsContext()
        {
            Database.SetInitializer<MyThingsContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Tussentabellen
            //modelBuilder.Entity<Sensor>()
            //    .HasMany(r => r.Containers)
            //    .WithMany()
            //    .Map( m => 
            //    {
            //        m.MapLeftKey("SensorId");
            //        m.MapRightKey("ContainerId");
            //        m.ToTable("SensorContainers");
            //    });
            modelBuilder.Entity<Group>()
                .HasMany(r => r.Sensors)
                .WithMany()
                .Map( m =>
                {
                    m.MapLeftKey("GroupId");
                    m.MapRightKey("SensorId");
                    m.ToTable("GroupedSensors");
                });

            //Remove any Circular References in Cascading
            modelBuilder.Entity<Error>()
                .HasRequired(e => e.Sensor)
                .WithMany()
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Error>()
                .HasRequired(e => e.Container)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
