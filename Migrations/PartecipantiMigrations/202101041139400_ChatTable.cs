namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChatTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Chat",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ChatID = c.String(),
                        IsTourneyChat = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Chat");
        }
    }
}
