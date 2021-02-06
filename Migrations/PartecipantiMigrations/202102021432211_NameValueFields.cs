namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NameValueFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Values", "Name", c => c.String());
            AddColumn("dbo.Values", "Value", c => c.Int(nullable: false));
            DropColumn("dbo.Values", "DataChanged");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Values", "DataChanged", c => c.Int(nullable: false));
            DropColumn("dbo.Values", "Value");
            DropColumn("dbo.Values", "Name");
        }
    }
}
