using Microsoft.Extensions.Logging.Abstractions;
using NameGate.BlockLists;

namespace NameGate.Tests
{
    public class OisdTests
    {
        private const string testList = """
# Version: 202401171529
# Title: oisd big
# Description: Block. Don't break.
# Last modified: 2024-01-17T15:29:23+0000
# Expires: 1 hours
# Homepage: https://oisd.nl
# License: https://github.com/sjhgvr/oisd/blob/main/LICENSE
# Syntax: Domains (wildcards) without *.
# Maintainer: Stephan van Ruth
# Contact: contact@oisd.nl

# All domains and their subdomains should be blocked
# Entry: "example.com" should block access to "example.com" and "subdomain.example.com" but not "thiseexample.com"

0--foodwarez.da.ru
0-24bpautomentes.hu
0-29.com
0-okodukai.com
0.101tubeporn.com
0.code.cotsta.ru
000.gaysexe.free.fr
000.ueuo.com
000002020303mbi3.rf.gd
000073736377.weebly.com
0001.2waky.com
0001proxy.notlong.com
0002yr2.wcomhost.com
000359.xyz
0008d6ba2e.com
000aproxy.on-4.com
000dn.com
000free.us
000lk3v.wcomhost.com
""";


        [Fact]
        public async Task TestBlockList()
        {
            var bl = GetBlockList();
            await bl.ForceLoadList(new StringReader(testList));

            await TestBlockListInternal(bl);
        }

        [Fact]
        public async Task TestBlockListDownload()
        {
            var bl = GetBlockList();
            await bl.RefreshCachedList();

            await TestBlockListInternal(bl);
        }


        private OisdBlockList GetBlockList()
        {
            var bl = new OisdBlockList(Services.GetServiceProvider(), new());
            return bl;
        }


        private async Task TestBlockListInternal(OisdBlockList bl)
        {
            Assert.True(await bl.DomainIsBlocked("000free.us"));
            Assert.True(await bl.DomainIsBlocked("test.000free.us"));
            Assert.False(await bl.DomainIsBlocked("0000free.us"));
        }
    }
}