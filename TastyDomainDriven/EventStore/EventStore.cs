namespace TastyDomainDriven.EventStore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class EventStore : IEventStore
    {
        protected static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(EventStore));

        protected IEventSerializer Formatter;

        protected readonly IAppendOnlyStore AppendOnlyStore;

        public EventStore CopyAndVersion(IAppendOnlyStore to, int version)
        {
            this.AppendOnlyStore.CopyAppender(to, version);
            return new EventStore(to);
        }

        public EventStore(IAppendOnlyStore appendOnlyStore)
        {
            if (appendOnlyStore == null)
            {
                throw new ArgumentNullException("appendOnlyStore");
            }

            this.Formatter = new BinaryFormatterSerializer();
            this.AppendOnlyStore = appendOnlyStore;
        }

        public EventStore(IAppendOnlyStore appendOnlyStore, IEventSerializer serializer)
        {
            if (appendOnlyStore == null)
            {
                throw new ArgumentNullException("appendOnlyStore");
            }

            this.Formatter = serializer;
            this.AppendOnlyStore = appendOnlyStore;
        }

        
        public EventStream ReplayAll(int? afterVersion = null, int? maxVersion = null)
        {
            var records = this.AppendOnlyStore.ReadRecords(afterVersion ?? 0, maxVersion ?? int.MaxValue).ToList();
            var stream = new EventStream();

            foreach (var tapeRecord in records)
            {
                stream.Events.AddRange(this.DeserializeEvent(tapeRecord.Data));
                stream.Version++;
            }

            return stream;
        }

        private IEnumerable<IEvent> DeserializeEvent(byte[] data)
        {
            return this.Formatter.DeserializeEvent(data);
        }

        private byte[] SerializeEvent(IEvent[] toArray)
        {
            return this.Formatter.SerializeEvent(toArray);
        }

        public EventStream LoadEventStream(IIdentity id, long skip, int take)
        {
            var name = this.IdentityToString(id);
            var records = this.AppendOnlyStore.ReadRecords(name, skip, take).ToList();
            var stream = new EventStream();

            foreach (var tapeRecord in records)
            {
                stream.Events.AddRange(this.DeserializeEvent(tapeRecord.Data));
                stream.Version = tapeRecord.Version;
            }
            return stream;
        }

        string IdentityToString(IIdentity id)
        {
            // in this project all identities produce proper name
            return id.ToString();
        }

        public EventStream LoadEventStream(IIdentity id)
        {
            EventStream loadEventStream = this.LoadEventStream(id, 0, int.MaxValue);
            Logger.DebugFormat("Loaded stream {0} on rev {1}", id, loadEventStream.Version);
            return loadEventStream;
        }
        
        public virtual void AppendToStream(IIdentity id, long originalVersion, ICollection<IEvent> events)
        {
            if (events.Count == 0)
            {
                return;
            }

            var name = this.IdentityToString(id);
            var data = this.SerializeEvent(events.ToArray());

            try
            {
                this.AppendOnlyStore.Append(name, data, originalVersion);
            }
            catch (AppendOnlyStoreConcurrencyException e)
            {
                // load server events
                var server = this.LoadEventStream(id, 0, int.MaxValue);
                // throw a real problem
                throw OptimisticConcurrencyException.Create(server.Version, e.ExpectedStreamVersion, id, server.Events, events.ToList());
            }

            // technically there should be a parallel process that queries new changes 
            // from the event store and sends them via messages (avoiding 2PC problem). 
            // however, for demo purposes, we'll just send them to the console from here
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var @event in events)
            {
                Logger.DebugFormat("{0} r{1} Event: {2}", id, originalVersion, @event);
            }
        }
    }
}