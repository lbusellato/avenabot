using avenabot.Models.Eliminatorie;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace avenabot.DAL
{
    [DbConfigurationType(typeof(EliminatorieDbConfig))]
    public class EliminatorieDbContext : DbContext
    {
        public static string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\loren\\Documents\\Eliminatorie.mdf;Integrated Security = True; Connect Timeout = 30";

        public EliminatorieDbContext() : base(connString)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<GironeADbContext, avenabot.Migrations.GironeAMigrations.Configuration>());
        }

        public DbSet<Quarti> Quarti { get; set; }
        public DbSet<Semifinali> Semifinali { get; set; }
        public DbSet<Finale> Finale { get; set; }
        public DbSet<Consolazione> Consolazione { get; set; }
        public DbSet<Campione> Campione { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
