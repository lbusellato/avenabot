namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConsolazioneTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Consolazione",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Results = c.String(),
                        OpponentEID = c.Int(nullable: false),
                        EID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Consolazione");
        }
    }
}
