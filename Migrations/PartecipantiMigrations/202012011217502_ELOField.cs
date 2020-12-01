namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ELOField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partecipante", "ELO", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partecipante", "ELO");
        }
    }
}
