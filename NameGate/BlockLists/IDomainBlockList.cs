namespace NameGate.BlockLists
{
    public interface IDomainBlockList
    {
        Task<bool> DomainIsBlocked(string domain);
        Task RefreshCachedList();
    }
}
