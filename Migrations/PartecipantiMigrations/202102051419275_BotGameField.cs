namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BotGameField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partecipante", "BotGame", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partecipante", "BotGame");
        }
    }
}
