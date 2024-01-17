using Microsoft.Extensions.Logging.Abstractions;
using NameGate.BlockLists;

namespace NameGate.Tests
{
    public class FireBogTests
    {
        [Fact]
        public async Task TestBlockListDownload()
        {
            var bl = GetBlockList();
            await bl.RefreshCachedList();

            await TestBlockListInternal(bl);
        }


        private FireBogBlockList GetBlockList()
        {
            var bl = new FireBogBlockList(Services.GetServiceProvider(), new());
            return bl;
        }


        private async Task TestBlockListInternal(IDomainBlockList bl)
        {
            Assert.True(await bl.DomainIsBlocked("4life.com"));
            Assert.False(await bl.DomainIsBlocked("test.4life.com"));
            Assert.False(await bl.DomainIsBlocked("a4life.com"));
        }
    }
}