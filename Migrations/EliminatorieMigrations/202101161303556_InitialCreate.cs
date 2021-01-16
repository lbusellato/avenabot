namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Finale",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Results = c.String(),
                        OpponentEID = c.Int(nullable: false),
                        EID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Ottavi",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Results = c.String(),
                        OpponentEID = c.Int(nullable: false),
                        EID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Quarti",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        PlayerID = c.Int(nullable: false),
                        Results = c.String(),
                        OpponentEID = c.Int(nullable: false),
                        EID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Semifinali",
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
            DropTable("dbo.Semifinali");
            DropTable("dbo.Quarti");
            DropTable("dbo.Ottavi");
            DropTable("dbo.Finale");
        }
    }
}
