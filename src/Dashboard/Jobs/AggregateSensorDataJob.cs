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
        private readonly IDataAggregationService _dataAggregationService;

        /// <summary>
        /// Aggregate Sensor Data Job
        /// </summary>
        public AggregateSensorDataJob(IDataAggregationService dataAggregationService)
        {
            this._dataAggregationService = dataAggregationService;
        }

        /// <inheritdoc/>
        public async Task Execute(IJobExecutionContext context)
        {
            await this._dataAggregationService.AggregateAsync(context.CancellationToken);
        }
    }
}
