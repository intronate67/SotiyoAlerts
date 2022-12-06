using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SotiyoAlerts.Data
{
    public partial class SotiyoAlertsDb
    {
        partial void CustomInit(DbContextOptionsBuilder optionsBuilder)
        {
            ConfigureOptions(optionsBuilder);
        }

        partial void OnModelCreatedImpl(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.ChannelFilter>()
                .HasOne(typeof(Models.SubFilter))
                .WithMany()
                .HasForeignKey("SubFilterId")
                .OnDelete(DeleteBehavior.Restrict);
        }

        public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("SotiyoAlertsDb");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
