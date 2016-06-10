namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimeholderAdded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Timeholders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WebjobInstanceStarted = c.DateTime(nullable: false),
                        WebjobInstanceEnded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Timeholders");
        }
    }
}
