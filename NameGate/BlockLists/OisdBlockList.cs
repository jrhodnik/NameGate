using NameGate.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace NameGate.BlockLists
{
    public class OisdBlockList : HttpFileBasedBlockList
    {
        public OisdBlockList(IServiceProvider serviceProvider, FileBlockListConfig config)
            : base("https://big.oisd.nl/domainswild2", config.CacheExpiry, serviceProvider)
        {
        }

        protected override Task<bool> MatchDomain(string blockListedDomain, string domain)
        {
            if (domain.Equals(blockListedDomain, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(true);

            if (domain.EndsWith($".{blockListedDomain}", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        protected override async IAsyncEnumerable<string> ParseList(TextReader tr)
        {
            int count = 0;
            while (true)
            {
                var line = await tr.ReadLineAsync();
                if (line == null)
                    break;

                if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();

                if (Uri.CheckHostName(line) == UriHostNameType.Unknown)
                {
                    Logger.LogWarning("Invalid domain: {domain}", line);
                    continue;
                }

                count++;
                yield return line;
            }

            Logger.LogInformation("Loaded {count} blocked domains.", count);
        }
    }
}
