using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public class EventStoreForAsyncProjection : IEventStore
	{
		private readonly IEventStore eventStore;
	    private readonly IAsyncProjection projection;

	    public EventStoreForAsyncProjection(IEventStore source, IAsyncProjection projection)
	    {
	        this.eventStore = source;
	        this.projection = projection;
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
            // Next step make this async and IEventStore can be changed to returning Task
			this.eventStore.AppendToStream(id, expectedVersion, events);

            //Step1 - made async
            Task.Run(async () =>
            {
                foreach (var e in events)
                {
                    await projection.Consume(e);
                }
            }).Wait();

        }

		public EventStream ReplayAll(int? afterVersion = null, int? maxVersion = null)
		{
			return this.eventStore.ReplayAll(afterVersion, maxVersion);
		}
	}
}