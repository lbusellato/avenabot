using avenabot.Models.Membri;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace avenabot.DAL
{
    [DbConfigurationType(typeof(MembriDbConfiguration))]
    public class MembriDbContext : DbContext
    {
        public static string connString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\loren\\Documents\\Membri.mdf;Integrated Security = True; Connect Timeout = 30";

        public MembriDbContext() : base(connString)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<PartecipantiDbContext, avenabot.Migrations.PartecipantiMigrations.Configuration>());
        }

        public DbSet<Membro> Membri { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}