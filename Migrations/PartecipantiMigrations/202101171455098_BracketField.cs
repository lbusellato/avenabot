namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BracketField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partecipante", "Bracket", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partecipante", "Bracket");
        }
    }
}
