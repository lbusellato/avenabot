using avenabot.Models.Gironi;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace avenabot.DAL
{
    [DbConfigurationType(typeof(GironeDbConfiguration))]
    public class GironeFDbContext : DbContext
    {
        public static string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\loren\\Documents\\GironeF.mdf;Integrated Security = True; Connect Timeout = 30";

        public GironeFDbContext() : base(connString)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<GironeADbContext, avenabot.Migrations.GironeAMigrations.Configuration>());
        }

        public DbSet<Girone> Girone { get; set; }
        public DbSet<Game> Partite { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}