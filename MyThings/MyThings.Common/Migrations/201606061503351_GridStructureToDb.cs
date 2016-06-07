namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GridStructureToDb : DbMigration
    {
        public override void Up()
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
            
            CreateTable(
                "dbo.Tiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Col = c.Int(nullable: false),
                        Row = c.Int(nullable: false),
                        Size_X = c.Single(nullable: false),
                        Size_Y = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserTiles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        TileId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.TileId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.Tiles", t => t.TileId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.TileId);
            
            AddColumn("dbo.AspNetUsers", "RawGridsterJson", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserTiles", "TileId", "dbo.Tiles");
            DropForeignKey("dbo.UserTiles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserTiles", new[] { "TileId" });
            DropIndex("dbo.UserTiles", new[] { "UserId" });
            DropColumn("dbo.AspNetUsers", "RawGridsterJson");
            DropTable("dbo.UserTiles");
            DropTable("dbo.Tiles");
            DropTable("dbo.Pins");
        }
    }
}
