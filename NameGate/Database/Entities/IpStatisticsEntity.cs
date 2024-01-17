namespace NameGate.Database.Entities
{
    public record IpStatisticsEntity
    {
        public required string IpAddress { get; init; }
        public ulong AllowedCount { get; set; } = 0;
        public ulong BlockedCount { get; set; } = 0;
    };
}
