namespace NameGate.Database.Entities
{
    public record DomainStatisticsEntity
    {
        public required string Domain { get; init; }
        public ulong AllowedCount { get; set; } = 0;
        public ulong BlockedCount { get; set; } = 0;
    }
}
