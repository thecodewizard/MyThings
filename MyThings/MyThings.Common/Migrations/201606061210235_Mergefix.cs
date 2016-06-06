namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Mergefix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Containers", "MACAddress", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Containers", "MACAddress");
        }
    }
}
