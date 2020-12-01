namespace avenabot.Migrations.PartecipantiMigrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<avenabot.DAL.PartecipantiDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\PartecipantiMigrations";
            ContextKey = "avenabot.DAL.PartecipantiDbContext";
        }

        protected override void Seed(avenabot.DAL.PartecipantiDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
