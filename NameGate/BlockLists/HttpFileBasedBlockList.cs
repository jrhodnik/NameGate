using Microsoft.Extensions.DependencyInjection;
using NameGate.Services;

namespace NameGate.BlockLists
{
    public abstract class HttpFileBasedBlockList : IDomainBlockList
    {
        protected readonly string Url;
        protected readonly HttpClient Client;
        protected readonly FileCacher FileCache;
        protected readonly ILogger Logger;
        private List<string> _blockedDomains = new();

        public HttpFileBasedBlockList(string url, TimeSpan expiry, IServiceProvider serviceProvider)
        {
            Url = url;
            Client = serviceProvider.GetRequiredService<HttpClient>();

            var directoryManager = serviceProvider.GetRequiredService<DirectoryManager>();

            FileCache = new FileCacher(directoryManager.GetCacheFilePath($"{GetType().Name}.txt"), expiry, Loader);
            Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName ?? GetType().Name);
        }

        public async Task<bool> DomainIsBlocked(string domain)
        {
            foreach (var d in _blockedDomains)
            {
                if (await MatchDomain(d, domain))
                    return true;
            }

            return false;
        }

        public async Task RefreshCachedList()
        {
            try
            {
                if (await FileCache.Refresh() || _blockedDomains.Count == 0)
                {
                    var s = await FileCache.GetStream();
                    using var sr = new StreamReader(s);
                    _blockedDomains = await ParseList(sr).ToListAsync();
                }
            }
            catch(Exception ex)
            {
                Logger.LogWarning(ex, "Error refreshing list.");
            }
        }

        public async Task ForceLoadList(TextReader tr)
        {
            _blockedDomains = await ParseList(tr).ToListAsync();
        }

        protected virtual Task<Stream> Loader() => Client.GetStreamAsync(Url);
        protected abstract IAsyncEnumerable<string> ParseList(TextReader tr);
        protected abstract Task<bool> MatchDomain(string blockListedDomain, string domain);
    }
}
