using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public class EventStoreAsync : IEventStoreAsync
    {
        private readonly IAppendOnlyAsync appender;
        private readonly IEventSerializer serializer;

        protected static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(EventStoreAsync));

        public EventStoreAsync(IAppendOnlyAsync appender, IEventSerializer serializer = null)
        {
            this.appender = appender;
            this.serializer = serializer ?? new BinaryFormatterSerializer();
        }

        public async Task AppendToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events)
        {
            if (events.Count == 0)
            {
                return;
            }

            var name = id.ToString();
            var data = this.serializer.SerializeEvent(events.ToArray());

            try
            {
                await this.appender.Append(name, data, expectedVersion);
            }
            catch (AppendOnlyStoreConcurrencyException e)
            {
                // load server events
                var server = await this.LoadEventStream(id, 0, int.MaxValue);
                // throw a real problem
                throw OptimisticConcurrencyException.Create(server.Version, e.ExpectedStreamVersion, id, server.Events);
            }

            foreach (var @event in events)
            {
                Logger.DebugFormat("{0} r{1} Event: {2}", id, expectedVersion, @event);
            }
        }

        public async Task<EventStream> LoadEventStream(IIdentity id)
        {
            EventStream loadEventStream = await this.LoadEventStream(id, 0, int.MaxValue);
            Logger.DebugFormat("Loaded stream {0} on rev {1}", id, loadEventStream.Version);
            return loadEventStream;
        }

        public async Task<EventStream> LoadEventStream(IIdentity id, long skip, int take)
        {
            var name = id.ToString();
            var records = (await this.appender.ReadRecords(name, skip, take)).ToList();
            var stream = new EventStream();

            foreach (var tapeRecord in records)
            {
                stream.Events.AddRange(this.serializer.DeserializeEvent(tapeRecord.Data));
                stream.Version = tapeRecord.Version;
            }
            return stream;
        }

        public async Task<EventStream> ReplayAll(int? afterVersion = default(int?), int? maxVersion = default(int?))
        {
            var records = (await this.appender.ReadRecords(afterVersion ?? 0, maxVersion ?? int.MaxValue)).ToList();
            var stream = new EventStream();

            foreach (var tapeRecord in records)
            {
                stream.Events.AddRange(this.serializer.DeserializeEvent(tapeRecord.Data));
                stream.Version++;
            }

            return stream;
        }
    }    
}