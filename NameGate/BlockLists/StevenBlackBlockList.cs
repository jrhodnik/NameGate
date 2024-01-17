
using NameGate.Services;

namespace NameGate.BlockLists
{
    public class StevenBlackBlockList : HttpFileBasedBlockList
    {
        public StevenBlackBlockList(IServiceProvider sp, FileBlockListConfig config)
            : base("https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts", config.CacheExpiry, sp)
        {
        }

        protected override Task<bool> MatchDomain(string blockListedDomain, string domain)
        {
            if (domain.Equals(blockListedDomain, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        protected override async IAsyncEnumerable<string> ParseList(TextReader tr)
        {
            int count = 0;
            bool hasStarted = false;
            while (true)
            {
                var line = await tr.ReadLineAsync();
                if (line == null)
                    break;

                if (line.Equals("# Start StevenBlack", StringComparison.OrdinalIgnoreCase))
                    hasStarted = true;

                if (!hasStarted)
                    continue;

                line = line.Trim();

                if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                    continue;

                var spl = line.Split(' ');

                if (spl.Length < 2)
                    continue;

                var domain = spl[1];

                if (Uri.CheckHostName(domain) == UriHostNameType.Unknown)
                {
                    Logger.LogWarning("Invalid domain: {domain}", domain);
                    continue;
                }

                count++;
                yield return domain;
            }

            Logger.LogInformation("Loaded {count} blocked domains.", count);
        }
    }
}
