namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveIsTourneyField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Chat", "IsTourneyChat");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Chat", "IsTourneyChat", c => c.Boolean(nullable: false));
        }
    }
}
