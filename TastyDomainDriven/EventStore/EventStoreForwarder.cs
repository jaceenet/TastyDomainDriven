namespace TastyDomainDriven.EventStore
{
	using System;
	using System.Collections.Generic;

	public class EventStoreForwarder : EventStore
    {
        public EventStoreForwarder(IAppendOnlyStore appendOnlyStore, Action<IEvent> forward = null)
            : base(appendOnlyStore)
        {
            this.Forward = forward;
        }

        public Action<IEvent> Forward { get; set; }

        public override void AppendToStream(IIdentity id, long originalVersion, ICollection<IEvent> events)
        {
            base.AppendToStream(id, originalVersion, events);

            if (this.Forward != null)
            {
                foreach (var e in events)
                {
                    this.Forward(e);
                }
            }
        }
    }
}