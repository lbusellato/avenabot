namespace avenabot.Migrations.MembriMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TGIDField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Membro", "TGID", c => c.String());
            DropColumn("dbo.Membro", "TID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Membro", "TID", c => c.Int(nullable: false));
            DropColumn("dbo.Membro", "TGID");
        }
    }
}
