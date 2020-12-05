namespace avenabot.Migrations.GironeBMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Girone", "GID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Girone", "GID");
        }
    }
}
