namespace MyThings.Common.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedthresholds : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Thresholds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BetweenValuesActive = c.Boolean(nullable: false),
                        MinValue = c.Double(nullable: false),
                        MaxValue = c.Double(nullable: false),
                        MatchValueActive = c.Boolean(nullable: false),
                        MatchValue = c.String(),
                        FrequencyActive = c.Boolean(nullable: false),
                        MinUpdateInterval = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Containers", "ThresholdId", c => c.Int());
            CreateIndex("dbo.Containers", "ThresholdId");
            AddForeignKey("dbo.Containers", "ThresholdId", "dbo.Thresholds", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Containers", "ThresholdId", "dbo.Thresholds");
            DropIndex("dbo.Containers", new[] { "ThresholdId" });
            DropColumn("dbo.Containers", "ThresholdId");
            DropTable("dbo.Thresholds");
        }
    }
}
