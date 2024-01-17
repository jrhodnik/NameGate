using Microsoft.EntityFrameworkCore;
using NameGate.Database.Entities;

namespace NameGate.Database.Repositories
{
    public interface IIpStatisticsRepository
    {
        Task Increment(IEnumerable<IpStatisticsEntity> ipStatistics);
        IAsyncEnumerable<IpStatisticsEntity> Get();
    }

    public class EfcoreIpStatisticsRepository(MainContext _db) : IIpStatisticsRepository
    {
        private DbSet<IpStatisticsEntity> Table => _db.IpStatistics;

        public IAsyncEnumerable<IpStatisticsEntity> Get() => Table.AsAsyncEnumerable();

        public async Task Increment(IEnumerable<IpStatisticsEntity> ipStatistics)
        {
            foreach(var entry in ipStatistics)
            {
                var dbEntry = await Table.SingleOrDefaultAsync(x => x.IpAddress == entry.IpAddress);

                if (dbEntry == null)
                {
                    dbEntry = new() { IpAddress = entry.IpAddress };
                    Table.Add(dbEntry);
                }

                dbEntry.AllowedCount += entry.AllowedCount;
                dbEntry.BlockedCount += entry.BlockedCount;
            }

            await _db.SaveChangesAsync();
        }
    }
}
