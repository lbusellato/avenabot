namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChatIDType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Chat", "ChatID", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Chat", "ChatID", c => c.String());
        }
    }
}
