using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.PerformanceMeasurements;

namespace TastyDomainDriven.AggregateService
{
    public class UpdateAggregateAsync<TAggregateRoot> where TAggregateRoot : IAggregate, new()
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(UpdateAggregate<TAggregateRoot>));
        private IPerformanceLogger performanceLogger;

        public UpdateAggregateAsync(IPerformanceLogger logger = null)
        {
            performanceLogger = logger;
        }

        public async Task Execute<TIdent>(IEventStoreAsync eventStorage, TIdent id, Action<TAggregateRoot> execute) where TIdent : IIdentity
        {
            Stopwatch stLoad, stHistory, stExecute, stAppend;

            if (eventStorage == null)
            {
                throw new ArgumentNullException("eventStorage");
            }

            if (id == null || id.Equals(default(TIdent)))
            {
                throw new ArgumentException("id is null or default value", "id");
            }

            stLoad = Stopwatch.StartNew();
            // Load event stream from the store
            var stream = await eventStorage.LoadEventStream(id);
            stLoad.Stop();

            stHistory = Stopwatch.StartNew();
            // create new Customer aggregate from the history
            var aggregate = new TAggregateRoot();
            aggregate.LoadsFromHistory(stream.Events);
            stHistory.Stop();

            // execute delegated action
            Logger.DebugFormat("Executing Update on aggregate {0}", aggregate);

            stExecute = Stopwatch.StartNew();
            execute(aggregate);
            stExecute.Stop();

            stAppend = Stopwatch.StartNew();
            // append resulting changes to the stream, expect the same version loaded before applying ddd logic
            if (aggregate.Changes.Any())
            {
                Logger.DebugFormat("Saving {0} uncommited events on aggregate {1}", aggregate.Changes.Count, aggregate);
                await eventStorage.AppendToStream(id, stream.Version, aggregate.Changes);
            }
            stAppend.Stop();

            Logger.DebugFormat("Finished Update on {0}", aggregate);

            if (performanceLogger != null)
            {
                AggregatePerformance performance = new AggregatePerformance()
                {
                    AggregateType = GetType(),
                    LoadEventsTime = stLoad.Elapsed,
                    RestoreStateTime = stHistory.Elapsed,
                    ExecuteAggregateTime = stExecute.Elapsed,
                    SaveChangesTime = stAppend.Elapsed,
                    HistoryEventsCount = stream.Events.Count,
                    NewEventsCount = aggregate.Changes.Count
                };
                await performanceLogger.TrackAggregate(performance);
            }
        }
    }
}