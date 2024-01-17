using Microsoft.EntityFrameworkCore;
using NameGate.Database.Entities;

namespace NameGate.Database.Repositories
{
    public interface IDomainStatisticsRepository
    {
        Task Increment(IEnumerable<DomainStatisticsEntity> entity);
        IAsyncEnumerable<DomainStatisticsEntity> Get();
    }

    public class EfcoreDomainStatisticsRepository(MainContext _db) : IDomainStatisticsRepository
    {
        private DbSet<DomainStatisticsEntity> Table => _db.DomainStatistics;

        public IAsyncEnumerable<DomainStatisticsEntity> Get() => Table.AsAsyncEnumerable();

        public async Task Increment(IEnumerable<DomainStatisticsEntity> entities)
        {
            foreach (var entry in entities)
            {
                var dbEntry = await Table.SingleOrDefaultAsync(x => x.Domain == entry.Domain);

                if (dbEntry == null)
                {
                    dbEntry = new() { Domain = entry.Domain }; 
                    Table.Add(dbEntry);
                }

                dbEntry.AllowedCount += entry.AllowedCount;
                dbEntry.BlockedCount += entry.BlockedCount;
            }

            await _db.SaveChangesAsync();
        }
    }
}
