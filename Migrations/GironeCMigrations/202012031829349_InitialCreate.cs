namespace avenabot.Migrations.GironeCMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Girone",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Results = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Girone");
        }
    }
}
