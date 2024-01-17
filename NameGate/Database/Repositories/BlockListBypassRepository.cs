using Microsoft.EntityFrameworkCore;
using NameGate.Database;
using NameGate.Database.Entities;

namespace NameGate.Database.Repositories
{
    public interface IBlockListBypassRepository
    {
        public IAsyncEnumerable<BlockListBypassEntity> Get();
        Task Add(BlockListBypassEntity bypass);
        Task Remove(Guid id);
        Task Remove(string hostGlob);
    }

    public class EfcoreBlockListBypassRepository(MainContext _db) : IBlockListBypassRepository
    {
        private DbSet<BlockListBypassEntity> Table => _db.BlockListBypass;

        public IAsyncEnumerable<BlockListBypassEntity> Get() => Table.AsAsyncEnumerable();

        public async Task Add(BlockListBypassEntity bypass)
        {
            Table.Add(bypass);
            await _db.SaveChangesAsync();
        }

        public async Task Remove(Guid id)
        {
            var res = await Table.SingleOrDefaultAsync(x => x.Id == id);

            if (res == null)
                return;

            Table.Remove(res);
            await _db.SaveChangesAsync();
        }

        public async Task Remove(string hostGlob)
        {
            var res = await Table.SingleOrDefaultAsync(x => x.HostGlob == hostGlob);

            if (res == null)
                return;

            Table.Remove(res);
            await _db.SaveChangesAsync();
        }
    }
}
