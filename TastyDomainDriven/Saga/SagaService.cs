namespace TastyDomainDriven.Saga
{
    using System;
    using System.Collections;
    using System.Linq;

    [Obsolete("Not recommended saga implementation")]
    public class SagaService<TSagaAggregate>  where TSagaAggregate : ISagaRoot, new()
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(SagaService<TSagaAggregate>));

        private readonly ISagaEventStore eventStore;

        private readonly IBus bus;

        public SagaService(ISagaEventStore eventStore, IBus bus)
        {
            this.eventStore = eventStore;
            this.bus = bus;
        }

        public void Update<TIdent>(TIdent id, Action<TSagaAggregate> execute) where TIdent : IIdentity
        {
            // Load event stream from the store
            EventStream stream = this.eventStore.LoadEventStream(id);

            // create new Customer aggregate from the history
            var aggregate = new TSagaAggregate();
            QueueBus queueBus = new QueueBus();
            aggregate.Bus = queueBus; 
            aggregate.LoadsFromHistory(stream.Events);

            // execute delegated action
            Logger.DebugFormat("Executing Update on aggregate {0}", aggregate);
            execute(aggregate);

            // append resulting changes to the stream, expect the same version loaded before applying ddd logic
            if (aggregate.Changes.Any())
            {
                Logger.DebugFormat("Saving {0} uncommited events on aggregate {1}", aggregate.Changes.Count, aggregate);
                this.eventStore.AppendToStream(id, stream.Version, aggregate.Changes);
            }

            queueBus.EmptyQueue(this.bus);

            Logger.DebugFormat("Finished Update on {0}", aggregate);
        }
    }
}