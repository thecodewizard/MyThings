namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVirtualSensorFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Sensors", "IsVirtual", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "IsChanged", c => c.Boolean(nullable: false));
            AddColumn("dbo.Groups", "VirtualSensorIdentifier", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "VirtualSensorIdentifier");
            DropColumn("dbo.Groups", "IsChanged");
            DropColumn("dbo.Sensors", "IsVirtual");
        }
    }
}
