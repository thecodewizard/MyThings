namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GridStructureUpdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserTiles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserTiles", "TileId", "dbo.Tiles");
            DropIndex("dbo.UserTiles", new[] { "UserId" });
            DropIndex("dbo.UserTiles", new[] { "TileId" });
            AddColumn("dbo.AspNetUsers", "GridsterJson", c => c.String());
            DropColumn("dbo.AspNetUsers", "RawGridsterJson");
            DropTable("dbo.Tiles");
            DropTable("dbo.UserTiles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserTiles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        TileId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.TileId });
            
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
            
            AddColumn("dbo.AspNetUsers", "RawGridsterJson", c => c.String());
            DropColumn("dbo.AspNetUsers", "GridsterJson");
            CreateIndex("dbo.UserTiles", "TileId");
            CreateIndex("dbo.UserTiles", "UserId");
            AddForeignKey("dbo.UserTiles", "TileId", "dbo.Tiles", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserTiles", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
