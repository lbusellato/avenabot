using avenabot.Models.Partecipanti;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace avenabot.DAL
{
    [DbConfigurationType(typeof(PartecipantiDbConfiguration))]
    public class PartecipantiDbContext : DbContext
    {
        public static string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\loren\\Documents\\Partecipanti.mdf;Integrated Security = True; Connect Timeout = 30";

        public PartecipantiDbContext() : base(connString)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<PartecipantiDbContext, avenabot.Migrations.PartecipantiMigrations.Configuration>());
        }

        public DbSet<Partecipante> Partecipanti { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}