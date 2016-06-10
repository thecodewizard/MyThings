namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedgroupuserid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Groups", "User_Id", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Groups", "User_Id");
        }
    }
}
