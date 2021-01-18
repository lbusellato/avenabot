namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Finale", "FID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Finale", "FID");
        }
    }
}
