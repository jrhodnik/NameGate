using Microsoft.EntityFrameworkCore;
using NameGate.Database.Entities;

namespace NameGate.Database.Repositories
{
    public interface IWhiteListRepository
    {
        Task Add(WhiteListEntryEntity entry);
        IAsyncEnumerable<WhiteListEntryEntity> Get();
        Task Remove(Guid id);
        Task Remove(string domainGlob);
    }

    public class EfcoreWhiteListRepository(MainContext Db) : IWhiteListRepository
    {
        private DbSet<WhiteListEntryEntity> WhiteListEntries => Db.WhiteListEntries;

        public async Task Add(WhiteListEntryEntity entry)
        {
            WhiteListEntries.Add(entry);
            await Db.SaveChangesAsync();
        }

        public IAsyncEnumerable<WhiteListEntryEntity> Get() => WhiteListEntries.AsAsyncEnumerable();

        public async Task Remove(Guid id)
        {
            var res = await WhiteListEntries.SingleOrDefaultAsync(x => x.Id == id);

            if (res == null)
                return;

            WhiteListEntries.Remove(res);
            await Db.SaveChangesAsync();
        }

        public async Task Remove(string domainGlob)
        {
            var res = await WhiteListEntries.SingleOrDefaultAsync(x => x.DomainGlob == domainGlob);

            if (res == null)
                return;

            WhiteListEntries.Remove(res);
            await Db.SaveChangesAsync();
        }
    }
}
