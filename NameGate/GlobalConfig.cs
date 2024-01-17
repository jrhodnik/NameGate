using System.Security.Cryptography;

namespace NameGate
{
    public record GlobalConfig
    {
        public required string[] DnsServers { get; init; } = [];

        public required string? DataDirectory { get; init; } = null;
        public required string? CacheDirectory { get; init; } = null;
        public string? ReverseLookupServer { get; init; } = null;
        public required FileBlockListConfig? OisdConfig { get; init; } = null;
        public required FileBlockListConfig? StevenBlackConfig { get; init; } = null;
        public required FileBlockListConfig? FireBogConfig { get; init; } = null;
    }

    public record FileBlockListConfig
    {
        /// <summary>
        /// Whether or not to use oisd.
        /// </summary>
        public bool Enabled { get; init; } = true;


        /// <summary>
        /// When we expire the on-disk cache list.
        /// </summary>
        public TimeSpan CacheExpiry { get; init; } = TimeSpan.FromDays(1);
    }
}
