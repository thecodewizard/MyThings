namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigratedPinstoTablestorage : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "GridsterJson");
            DropTable("dbo.Pins");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Pins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        TileId = c.Int(nullable: false),
                        SavedId = c.Int(nullable: false),
                        SavedType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "GridsterJson", c => c.String());
        }
    }
}
