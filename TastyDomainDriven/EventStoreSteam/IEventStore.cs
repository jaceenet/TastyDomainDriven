namespace TastyDomainDriven
{
    using System.Collections.Generic;

    public interface IEventstoreStream
    {
        EventStream LoadEventStream(IIdentity id);

        //EventStream LoadEventStream(IIdentity id, long skipEvents, int maxCount);
        ///// <summary>
        ///// Appends events to server stream for the provided identity.
        ///// </summary>
        ///// <param name="id">identity to append to.</param>
        ///// <param name="expectedVersion">The expected version (specify -1 to append anyway).</param>
        ///// <param name="events">The events to append.</param>
        ///// <exception cref="OptimisticConcurrencyException">when new events were added to server
        ///// since <paramref name="expectedVersion"/>
        ///// </exception>
        //void AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events);

        //EventStream ReplayAll(int? afterVersion = null, int? maxVersion = null);
    }

    public class IEventStream
    {
        // version of the event stream returned
        long Version;
        
        // all events in the stream
        List<IEvent> Events = new List<IEvent>();
    }
}