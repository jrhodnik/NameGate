
using GlobExpressions;
using NameGate.BlockLists;
using NameGate.Database.Repositories;
using System.Net;
using System.Security.Cryptography;
using ZiggyCreatures.Caching.Fusion;

namespace NameGate.Services
{
    public class BlockListManager : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IServiceScope _scope;
        private readonly IFusionCache _cache;

        private readonly List<IDomainBlockList> _blockLists = new();

        public BlockListManager(IServiceProvider sp)
        {
            _sp = sp;
            _scope = sp.CreateScope();
            _cache = sp.GetRequiredService<IFusionCacheProvider>().GetCache("BlackListedNames");
        }

        public async Task<DomainBlockResult> DomainIsBlocked(string domain, IPEndPoint? endpoint)
        {
            async Task<DomainBlockResult> DoLookup()
            {
                foreach (var bl in _blockLists)
                {
                    if (await bl.DomainIsBlocked(domain))
                    {
                        return new(true, bl);
                    }
                }

                return new(false, null);
            }

            var res = await _cache.GetOrSetAsync<DomainBlockResult>(
                        domain,
                        async _ => await DoLookup(),
                        TimeSpan.FromMinutes(10)
                    ) ?? await DoLookup();

            using var scope = _sp.CreateScope();

            if (res.IsBlocked)
            {
                // WhiteList bypass, external to cache
                var whiteListRepo = scope.ServiceProvider.GetRequiredService<IWhiteListRepository>();
                if (await whiteListRepo.Get().AnyAsync(x => Glob.IsMatch(domain, x.DomainGlob, GlobOptions.CaseInsensitive)))
                    return new(false, null);

                if (endpoint != null)
                {
                    // BlockList bypass, external to cache
                    var blockListRepo = scope.ServiceProvider.GetRequiredService<IBlockListBypassRepository>();

                    var resolver = scope.ServiceProvider.GetRequiredService<HostNameResolver>();
                    var hostTask = resolver.GetHostEntryAsync(endpoint.Address);

                    if (await blockListRepo.Get().AnyAsync(x => Glob.IsMatch(endpoint.Address.ToString(), x.HostGlob)))
                        return new(false, null);

                    var host = await hostTask; //Start host lookup before we query the db above, to opimmize...
                    if (host != null && await blockListRepo.Get().AnyAsync(x => Glob.IsMatch(host.HostName, x.HostGlob, GlobOptions.CaseInsensitive)))
                        return new(false, null);
                }
            }
            return res;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = _scope.ServiceProvider.GetRequiredService<GlobalConfig>();

            if (config.OisdConfig != null && config.OisdConfig.Enabled)
            {
                var oisd = new OisdBlockList(_scope.ServiceProvider, config.OisdConfig);
                _blockLists.Add(oisd);
            }

            if(config.StevenBlackConfig != null && config.StevenBlackConfig.Enabled)
            {
                var sb = new StevenBlackBlockList(_scope.ServiceProvider, config.StevenBlackConfig);
                _blockLists.Add(sb);
            }

            if(config.FireBogConfig != null && config.FireBogConfig.Enabled)
            {
                var fb = new FireBogBlockList(_scope.ServiceProvider, config.FireBogConfig);
                _blockLists.Add(fb);
            }

            await Parallel.ForEachAsync(_blockLists, async (bl, ct) =>
            {
                await bl.RefreshCachedList();
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach(var bl in _blockLists)
                {
                    await bl.RefreshCachedList();
                }

                try
                {
                    await Task.Delay(10000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    //Normal
                }
            }
        }
    }

    public record DomainBlockResult(bool IsBlocked, IDomainBlockList? BlockList);
}
 