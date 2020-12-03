namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GironeField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Partecipante", "Girone", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Partecipante", "Girone");
        }
    }
}
