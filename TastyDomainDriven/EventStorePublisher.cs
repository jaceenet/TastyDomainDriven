namespace TastyDomainDriven
{
	using System.Collections.Generic;
	using System.Linq;

	public class EventStorePublisher : IEventStore
	{
		private readonly IEventStore eventStore;
	    private readonly IEventPublisher publisher;

	    public EventStorePublisher(IEventStore source, IEventPublisher publisher)
	    {
	        this.eventStore = source;
	        this.publisher = publisher;
	    }

	    public EventStream LoadEventStream(IIdentity id)
		{
			return this.eventStore.LoadEventStream(id);
		}

		public EventStream LoadEventStream(IIdentity id, long skipEvents, int maxCount)
		{
			return this.eventStore.LoadEventStream(id, skipEvents, maxCount);
		}

		public void AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events)
		{
			this.eventStore.AppendToStream(id, expectedVersion, events);
			this.publisher.ConsumeByReadSide(events.ToArray());
			this.publisher.ConsumeBySaga(events.ToArray());
		}

		public EventStream ReplayAll(int? afterVersion = null, int? maxVersion = null)
		{
			return this.eventStore.ReplayAll(afterVersion, maxVersion);
		}
	}
}