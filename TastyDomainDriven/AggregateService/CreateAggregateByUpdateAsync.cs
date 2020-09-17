using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.DI;
using TastyDomainDriven.PerformanceMeasurements;

namespace TastyDomainDriven.AggregateService
{
    public class CreateAggregateByUpdateAsync<TAggregateRoot> where TAggregateRoot : IAggregate
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(CreateAggregateByUpdate<TAggregateRoot>));
        private IPerformanceLogger performanceLogger;

        public CreateAggregateByUpdateAsync(IPerformanceLogger logger = null)
        {
            performanceLogger = logger;
        }

        public async Task Execute<TIdent, TResult, TIdentCreated>(IEventStoreAsync eventStorage, TIdent id, Func<TResult, TAggregateRoot> execute, TIdentCreated createId)
            where TIdent : IIdentity
            where TResult : IAggregate
            where TIdentCreated : IIdentity
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

            if (createId == null || createId.Equals(default(TIdentCreated)))
            {
                throw new ArgumentException("createid is null or default value", "createId");
            }
            
            stLoad = Stopwatch.StartNew();
            EventStream stream = await eventStorage.LoadEventStream(id);
            stLoad.Stop();

            stHistory = Stopwatch.StartNew();
            // create new Customer aggregate from the history
            TResult aggregate = CreateAggregate<TIdent, TResult, TIdentCreated>();
            aggregate.LoadsFromHistory(stream.Events);
            stHistory.Stop();

            // execute delegated action
            Logger.DebugFormat("Executing UpdateAndCreateAggregate on aggregate {0}", aggregate);

            stExecute = Stopwatch.StartNew();
            TAggregateRoot res = execute(aggregate);
            stExecute.Stop();

            if (aggregate.Changes.Any())
            {
                throw new Exception("You cannot modify more than one aggregate per command!");
            }

            stAppend = Stopwatch.StartNew();
            if (res != null && res.Changes.Any())
            {
                Logger.DebugFormat("Saving {0} uncommited events on result aggregate {1}", res.Changes.Count, res);
                await eventStorage.AppendToStream(createId, 0, res.Changes);
                Logger.DebugFormat("Finished Create on {0}", aggregate);
            }
            else
            {
                throw new InvalidOperationException("No aggregate created on execute");
            }
            stAppend.Stop();

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

        protected virtual TResult CreateAggregate<TIdent, TResult, TIdentCreated>() where TIdent : IIdentity where TResult : IAggregate where TIdentCreated : IIdentity
        {
            return ClassActivatorService.Instance.CreateInstance<TResult>();
        }
    }
}