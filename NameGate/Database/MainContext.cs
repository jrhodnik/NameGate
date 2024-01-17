using Microsoft.EntityFrameworkCore;
using NameGate.Database.Entities;
using NameGate.Services;

namespace NameGate.Database
{
    public class MainContext(DirectoryManager _directoryManager) : DbContext
    {
        public DbSet<WhiteListEntryEntity> WhiteListEntries { get; set; }
        public DbSet<IpStatisticsEntity> IpStatistics { get; set; }
        public DbSet<DomainStatisticsEntity> DomainStatistics { get; set; }
        public DbSet<BlockListBypassEntity> BlockListBypass { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_directoryManager.MainDatabasePath)!);
            optionsBuilder.UseSqlite($"Data Source={_directoryManager.MainDatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IpStatisticsEntity>().HasKey(x => x.IpAddress);
            modelBuilder.Entity<DomainStatisticsEntity>().HasKey(x => x.Domain);
        }
    }

    public class DatabaseBootstrapper : BackgroundService
    {
        private readonly IServiceScope _scope;
        private readonly MainContext _db;

        public DatabaseBootstrapper(IServiceProvider sp)
        {
            _scope = sp.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<MainContext>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _db.Database.MigrateAsync();
        }
    }
}
