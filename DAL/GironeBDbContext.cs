using avenabot.Models.Gironi;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace avenabot.DAL
{
    [DbConfigurationType(typeof(GironeDbConfiguration))]
    public class GironeBDbContext : DbContext
    {
        public static string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\loren\\Documents\\GironeB.mdf;Integrated Security = True; Connect Timeout = 30";

        public GironeBDbContext() : base(connString)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<GironeBDbContext, avenabot.Migrations.GironeBMigrations.Configuration>());
        }

        public DbSet<Girone> Girone { get; set; }
        public DbSet<Game> Partite { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}