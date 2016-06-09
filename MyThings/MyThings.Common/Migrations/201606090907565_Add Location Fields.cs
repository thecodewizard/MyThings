namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLocationFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sensors", "Lat", c => c.Double(nullable: false));
            AddColumn("dbo.Sensors", "Lng", c => c.Double(nullable: false));
            AddColumn("dbo.Sensors", "Accuracy", c => c.Int(nullable: false));
            DropColumn("dbo.Sensors", "BasestationLat");
            DropColumn("dbo.Sensors", "BasestationLng");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Sensors", "BasestationLng", c => c.Double(nullable: false));
            AddColumn("dbo.Sensors", "BasestationLat", c => c.Double(nullable: false));
            DropColumn("dbo.Sensors", "Accuracy");
            DropColumn("dbo.Sensors", "Lng");
            DropColumn("dbo.Sensors", "Lat");
        }
    }
}
