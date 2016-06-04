namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Containers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CreationTime = c.DateTime(nullable: false),
                        LastUpdatedTime = c.DateTime(nullable: false),
                        ContainerTypeId = c.Int(nullable: false),
                        SensorId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContainerTypes", t => t.ContainerTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Sensors", t => t.SensorId)
                .Index(t => t.ContainerTypeId)
                .Index(t => t.SensorId);
            
            CreateTable(
                "dbo.ContainerTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Sensors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Company = c.String(),
                        MACAddress = c.String(),
                        Location = c.String(),
                        CreationDate = c.DateTime(nullable: false),
                        SensorEntries = c.Long(nullable: false),
                        BasestationLat = c.Double(nullable: false),
                        BasestationLng = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Sensor_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sensors", t => t.Sensor_Id)
                .Index(t => t.Sensor_Id);
            
            CreateTable(
                "dbo.Errors",
                c => new
                    {
                        ErrorCode = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Category = c.Int(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        Advice = c.String(),
                        Time = c.DateTime(nullable: false),
                        Read = c.Boolean(nullable: false),
                        SensorId = c.Int(nullable: false),
                        ContainerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ErrorCode)
                .ForeignKey("dbo.Containers", t => t.ContainerId)
                .ForeignKey("dbo.Sensors", t => t.SensorId)
                .Index(t => t.SensorId)
                .Index(t => t.ContainerId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.SensorContainers",
                c => new
                    {
                        SensorId = c.Int(nullable: false),
                        ContainerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SensorId, t.ContainerId })
                .ForeignKey("dbo.Sensors", t => t.SensorId, cascadeDelete: true)
                .ForeignKey("dbo.Containers", t => t.ContainerId, cascadeDelete: true)
                .Index(t => t.SensorId)
                .Index(t => t.ContainerId);
            
            CreateTable(
                "dbo.GroupedSensors",
                c => new
                    {
                        GroupId = c.Int(nullable: false),
                        SensorId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.GroupId, t.SensorId })
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Sensors", t => t.SensorId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.SensorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Errors", "SensorId", "dbo.Sensors");
            DropForeignKey("dbo.Errors", "ContainerId", "dbo.Containers");
            DropForeignKey("dbo.Containers", "SensorId", "dbo.Sensors");
            DropForeignKey("dbo.Groups", "Sensor_Id", "dbo.Sensors");
            DropForeignKey("dbo.GroupedSensors", "SensorId", "dbo.Sensors");
            DropForeignKey("dbo.GroupedSensors", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.SensorContainers", "ContainerId", "dbo.Containers");
            DropForeignKey("dbo.SensorContainers", "SensorId", "dbo.Sensors");
            DropForeignKey("dbo.Containers", "ContainerTypeId", "dbo.ContainerTypes");
            DropIndex("dbo.GroupedSensors", new[] { "SensorId" });
            DropIndex("dbo.GroupedSensors", new[] { "GroupId" });
            DropIndex("dbo.SensorContainers", new[] { "ContainerId" });
            DropIndex("dbo.SensorContainers", new[] { "SensorId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Errors", new[] { "ContainerId" });
            DropIndex("dbo.Errors", new[] { "SensorId" });
            DropIndex("dbo.Groups", new[] { "Sensor_Id" });
            DropIndex("dbo.Containers", new[] { "SensorId" });
            DropIndex("dbo.Containers", new[] { "ContainerTypeId" });
            DropTable("dbo.GroupedSensors");
            DropTable("dbo.SensorContainers");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Errors");
            DropTable("dbo.Groups");
            DropTable("dbo.Sensors");
            DropTable("dbo.ContainerTypes");
            DropTable("dbo.Containers");
        }
    }
}
