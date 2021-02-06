namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveBracketField1 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Partecipante", "BotGame");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Partecipante", "BotGame", c => c.String());
        }
    }
}
