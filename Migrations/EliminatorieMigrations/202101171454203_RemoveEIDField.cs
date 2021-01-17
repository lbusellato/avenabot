namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveEIDField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Consolazione", "EID");
            DropColumn("dbo.Finale", "EID");
            DropColumn("dbo.Ottavi", "EID");
            DropColumn("dbo.Quarti", "EID");
            DropColumn("dbo.Semifinali", "EID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Semifinali", "EID", c => c.Int(nullable: false));
            AddColumn("dbo.Quarti", "EID", c => c.Int(nullable: false));
            AddColumn("dbo.Ottavi", "EID", c => c.Int(nullable: false));
            AddColumn("dbo.Finale", "EID", c => c.Int(nullable: false));
            AddColumn("dbo.Consolazione", "EID", c => c.Int(nullable: false));
        }
    }
}
