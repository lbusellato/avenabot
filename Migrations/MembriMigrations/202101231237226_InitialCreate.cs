namespace avenabot.Migrations.MembriMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Membro",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TID = c.Int(nullable: false),
                        LichessID = c.String(),
                        ELO = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Membro");
        }
    }
}
