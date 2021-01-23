namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveELOVarField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Partecipante", "ELOvar");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Partecipante", "ELOvar", c => c.Int(nullable: false));
        }
    }
}
