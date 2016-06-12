namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedSensorContainersTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SensorContainers", "SensorId", "dbo.Sensors");
            DropForeignKey("dbo.SensorContainers", "ContainerId", "dbo.Containers");
            DropIndex("dbo.SensorContainers", new[] { "SensorId" });
            DropIndex("dbo.SensorContainers", new[] { "ContainerId" });
            CreateIndex("dbo.Containers", "SensorId");
            AddForeignKey("dbo.Containers", "SensorId", "dbo.Sensors", "Id");
            DropTable("dbo.SensorContainers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SensorContainers",
                c => new
                    {
                        SensorId = c.Int(nullable: false),
                        ContainerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SensorId, t.ContainerId });
            
            DropForeignKey("dbo.Containers", "SensorId", "dbo.Sensors");
            DropIndex("dbo.Containers", new[] { "SensorId" });
            CreateIndex("dbo.SensorContainers", "ContainerId");
            CreateIndex("dbo.SensorContainers", "SensorId");
            AddForeignKey("dbo.SensorContainers", "ContainerId", "dbo.Containers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.SensorContainers", "SensorId", "dbo.Sensors", "Id", cascadeDelete: true);
        }
    }
}
