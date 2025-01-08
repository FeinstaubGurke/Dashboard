namespace Dashboard.Services
{
    public interface IDataAggregationService
    {
        Task<bool> AggregateAsync(CancellationToken cancellationToken = default);
    }
}
