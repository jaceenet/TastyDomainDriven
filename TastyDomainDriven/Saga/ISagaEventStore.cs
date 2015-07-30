namespace TastyDomainDriven.Saga
{
    using System;

    using TastyDomainDriven.EventStore;

	[Obsolete("Not recommended saga implementation", true)]
    public interface ISagaEventStore : IEventStore
    {
    }

    [Obsolete("Not recommended saga implementation", true)]
    public class SagaEventStore : EventStore, ISagaEventStore
    {
        public SagaEventStore(IAppendOnlyStore appendOnlyStore)
            : base(appendOnlyStore)
        {
        }
    }
}