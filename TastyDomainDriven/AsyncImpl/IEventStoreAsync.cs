using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public interface IEventStoreAsync
    {
        Task<EventStream> LoadEventStream(IIdentity id);
        Task<EventStream> LoadEventStream(IIdentity id, long skipEvents, int maxCount);
        /// <summary>
        /// Appends events to server stream for the provided identity.
        /// </summary>
        /// <param name="id">identity to append to.</param>
        /// <param name="expectedVersion">The expected version (specify -1 to append anyway).</param>
        /// <param name="events">The events to append.</param>
        /// <exception cref="OptimisticConcurrencyException">when new events were added to server
        /// since <paramref name="expectedVersion"/>
        /// </exception>
        Task AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events);

        Task<EventStream> ReplayAll(int? afterVersion = null, int? maxVersion = null);
    }
}