namespace avenabot.Migrations.GironeAMigrations
{
    using avenabot.Models.Gironi;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<avenabot.DAL.GironeADbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations/GironeAMigrations";
        }

        protected override void Seed(avenabot.DAL.GironeADbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            /*
            string test = "1,0,1,1,0,0,0.5,1,1";
            var risultati = new List<Girone>
            {
                new Girone { 
                    PlayerID = 2, 
                    Results = test
                },
            };
            foreach(Girone g in risultati)
            {
                context.GironeA.Add(g);
            }
            context.SaveChanges();
            */
        }
    }
}
