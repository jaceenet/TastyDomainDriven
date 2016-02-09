namespace TastyDomainDriven
{
    using System.Collections.Generic;

    public class EventStream
    {
        // version of the event stream returned
        public long Version;
        // all events in the stream
        public IEvent[] Events;
    }
}