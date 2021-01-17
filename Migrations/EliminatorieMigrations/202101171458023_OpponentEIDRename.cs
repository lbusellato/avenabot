namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OpponentEIDRename : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Consolazione", "OpponentID", c => c.Int(nullable: false));
            AddColumn("dbo.Finale", "OpponentID", c => c.Int(nullable: false));
            AddColumn("dbo.Ottavi", "OpponentID", c => c.Int(nullable: false));
            AddColumn("dbo.Quarti", "OpponentID", c => c.Int(nullable: false));
            AddColumn("dbo.Semifinali", "OpponentID", c => c.Int(nullable: false));
            DropColumn("dbo.Consolazione", "OpponentEID");
            DropColumn("dbo.Finale", "OpponentEID");
            DropColumn("dbo.Ottavi", "OpponentEID");
            DropColumn("dbo.Quarti", "OpponentEID");
            DropColumn("dbo.Semifinali", "OpponentEID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Semifinali", "OpponentEID", c => c.Int(nullable: false));
            AddColumn("dbo.Quarti", "OpponentEID", c => c.Int(nullable: false));
            AddColumn("dbo.Ottavi", "OpponentEID", c => c.Int(nullable: false));
            AddColumn("dbo.Finale", "OpponentEID", c => c.Int(nullable: false));
            AddColumn("dbo.Consolazione", "OpponentEID", c => c.Int(nullable: false));
            DropColumn("dbo.Semifinali", "OpponentID");
            DropColumn("dbo.Quarti", "OpponentID");
            DropColumn("dbo.Ottavi", "OpponentID");
            DropColumn("dbo.Finale", "OpponentID");
            DropColumn("dbo.Consolazione", "OpponentID");
        }
    }
}
