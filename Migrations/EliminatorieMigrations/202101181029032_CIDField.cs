namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Consolazione", "CID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Consolazione", "CID");
        }
    }
}
