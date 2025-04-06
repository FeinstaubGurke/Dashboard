using Dashboard.Services;
using Quartz;

namespace Dashboard.Jobs
{
    /// <summary>
    /// Aggregate Sensor Data Job
    /// </summary>
    [DisallowConcurrentExecution]
    public class AggregateSensorDataJob : IJob
    {
        private readonly ILogger<AggregateSensorDataJob> _logger;
        private readonly IDataAggregationService _dataAggregationService;

        /// <summary>
        /// Aggregate Sensor Data Job
        /// </summary>
        public AggregateSensorDataJob(
            ILogger<AggregateSensorDataJob> logger,
            IDataAggregationService dataAggregationService)
        {
            this._logger = logger;
            this._dataAggregationService = dataAggregationService;
        }

        /// <inheritdoc/>
        public async Task Execute(IJobExecutionContext context)
        {
            var isSuccessful = await this._dataAggregationService.AggregateAsync(context.CancellationToken);
            this._logger.LogInformation($"{nameof(Execute)} - Job done, isSuccessful:{isSuccessful}");
        }
    }
}
