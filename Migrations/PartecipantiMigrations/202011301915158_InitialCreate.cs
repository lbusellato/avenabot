namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Partecipante",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LichessID = c.String(),
                        TGID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Partecipante");
        }
    }
}
