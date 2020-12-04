namespace avenabot.Migrations.GironeAMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GamesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Game",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        P1ID = c.Int(nullable: false),
                        P2ID = c.Int(nullable: false),
                        Link = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Game");
        }
    }
}
