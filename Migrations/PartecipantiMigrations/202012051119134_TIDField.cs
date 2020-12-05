namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partecipante", "TID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partecipante", "TID");
        }
    }
}
