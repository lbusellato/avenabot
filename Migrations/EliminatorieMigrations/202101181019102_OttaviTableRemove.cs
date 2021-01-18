namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OttaviTableRemove : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Ottavi");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Ottavi",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Results = c.String(),
                        OpponentID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
