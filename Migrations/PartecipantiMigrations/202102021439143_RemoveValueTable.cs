namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveValueTable : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Values");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Values",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
    }
}
