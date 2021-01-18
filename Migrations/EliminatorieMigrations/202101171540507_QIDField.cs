namespace avenabot.Migrations.EliminatorieMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quarti", "QID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quarti", "QID");
        }
    }
}
