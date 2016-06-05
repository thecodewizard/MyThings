namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ErrorModelChanged : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.Errors");
            AddColumn("dbo.Errors", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Errors", "ErrorCode", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Errors", new[] { "Id", "ErrorCode" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.Errors");
            AlterColumn("dbo.Errors", "ErrorCode", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.Errors", "Id");
            AddPrimaryKey("dbo.Errors", "ErrorCode");
        }
    }
}
