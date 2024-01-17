
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using GlobExpressions;
using NameGate.Services;
using System.Globalization;
using System.Net;

namespace NameGate.BlockLists
{
    public class FireBogBlockList : HttpFileBasedBlockList
    {
        private readonly DirectoryManager _directoryManager;

        public FireBogBlockList(IServiceProvider sp, FileBlockListConfig config)
            : base("https://v.firebog.net/hosts/csv.txt", config.CacheExpiry, sp)
        {
            _directoryManager = sp.GetRequiredService<DirectoryManager>();
        }

        protected override Task<bool> MatchDomain(string blockListedDomain, string domain)
        {
            if (domain.Equals(blockListedDomain, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(true);

            if (domain.Contains("*") && Glob.IsMatch(domain, blockListedDomain, GlobOptions.CaseInsensitive))
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

                line = line.Trim();

                string[] invalidStarters = ["#", "!", "_"];
                if (invalidStarters.Any(line.StartsWith) || string.IsNullOrWhiteSpace(line))
                    continue;

                var spl = line.Split();

                string domain;

                if (IPAddress.TryParse(spl[0], out _) && spl.Length >= 2)
                    domain = spl[1];
                else
                    domain = spl[0];

                //For things like mydomain.com#comment
                domain = domain.Split('#').First();

                bool makeWildcard = false;
                if (domain.StartsWith("||"))
                {
                    domain = domain.Replace("||", "").Replace("^", "");
                }

            ValidateAndSubmit:
                if (string.IsNullOrWhiteSpace(domain))
                    continue;

                if (Uri.CheckHostName(domain) == UriHostNameType.Unknown)
                {
                    Logger.LogDebug("Invalid domain: {domain}", domain);
                    continue;
                }

                count++;
                yield return domain;

                if (makeWildcard)
                {
                    domain = $"*.{domain}";
                    goto ValidateAndSubmit;
                }
            }

            Logger.LogInformation("Loaded {count} blocked domains.", count);
        }

        protected override async Task<Stream> Loader()
        {
            var csvStream = await Client.GetStreamAsync(Url);
            using var tr = new StreamReader(csvStream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };

            using var reader = new CsvReader(tr, config);

            var ms = new MemoryStream();

            FireBogCsvRow temp = new();

            var recs = await reader.GetRecordsAsync<FireBogCsvRow>().Where(x => x.Type == "tick").ToListAsync();

            await Parallel.ForEachAsync(recs, async (rec, ct) =>
            {
                try
                {
                    var b = await Client.GetByteArrayAsync(rec.Url, ct);

                    lock (ms)
                    {
                        ms.Write(b);
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogWarning(ex, "Error fetching FireBog list.");
                }
            });

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private class FireBogCsvRow
        {
            [Index(0)]
            public string Category { get; set; } = "";

            [Index(1)]
            public string Type { get; set; } = "";

            [Index(2)]
            public string RepoUrl { get; set; } = "";

            [Index(3)]
            public string Name { get; set; } = "";

            [Index(4)]
            public string Url { get; set; } = "";
        }
    }
}
