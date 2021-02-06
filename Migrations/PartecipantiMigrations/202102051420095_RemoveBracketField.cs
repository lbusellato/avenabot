namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBracketField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Partecipante", "Bracket");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Partecipante", "Bracket", c => c.String());
        }
    }
}
