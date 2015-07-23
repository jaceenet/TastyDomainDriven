namespace TastyDomainDriven
{
	using System;
	using System.Collections.Generic;

	[Obsolete("Please use the EventStorePublisher instread.")]
	public abstract class BaseEventStoreProjector : IEventStore
	{
		private readonly IEventStore eventStore;

		protected BaseEventStoreProjector(IEventStore eventStore)
		{
			this.eventStore = eventStore;
		}

		public void ConsumeByReadSide(object @event)
		{
			((dynamic)this).ConsumeByReadSide((dynamic)@event);
		}

		public void ConsumeBySaga(object @event)
		{
			((dynamic)this).ConsumeBySaga((dynamic)@event);
		}

		public abstract void ConsumeByReadSide<T>(T e) where T : IEvent;

		public abstract void ConsumeBySaga<T>(T e) where T : IEvent;

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

			foreach (var @event in events)
			{
				((dynamic)this).ConsumeByReadSide((dynamic)@event);
			}

			foreach (var @event in events)
			{
				((dynamic)this).ConsumeBySaga((dynamic)@event);
			}
		}

		public EventStream ReplayAll(int? afterVersion = null, int? maxVersion = null)
		{
			return this.eventStore.ReplayAll(afterVersion, maxVersion);
		}
	}
}