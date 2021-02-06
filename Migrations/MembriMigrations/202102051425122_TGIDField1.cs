namespace avenabot.Migrations.MembriMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TGIDField1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Membro", "TGID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Membro", "TGID");
        }
    }
}
