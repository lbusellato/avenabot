namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Semifinali", "SID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Semifinali", "SID");
        }
    }
}
