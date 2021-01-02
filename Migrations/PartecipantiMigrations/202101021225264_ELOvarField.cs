namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ELOvarField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partecipante", "ELOvar", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partecipante", "ELOvar");
        }
    }
}
