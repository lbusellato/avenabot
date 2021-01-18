namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CampioneTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Campione",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Campione");
        }
    }
}
