namespace avenabot.Migrations.MembriMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTGIDField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Membro", "TGID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Membro", "TGID", c => c.String());
        }
    }
}
