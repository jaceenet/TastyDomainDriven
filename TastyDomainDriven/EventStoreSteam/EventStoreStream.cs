using System;
using System.Threading.Tasks;

namespace TastyDomainDriven.EventStoreSteam
{
    public class EventStoreStream
    {
        private readonly IAppendOnlyStoreStreamed appender;
        private readonly IEventRecordSerializer serializer;

        private Task<EventStream> Master(long skip = 0, long take = long.MaxValue)
        {
            return this.LoadEventStream(new StringId("master"), skip, take);
        }

        //public EventStream LoadEventStream(IIdentity id, long skip, int take)
        //{
        //    var name = this.IdentityToString(id);
        //    var records = this.AppendOnlyStore.ReadRecords(name, skip, take).ToList();
        //    var stream = new EventStream();

        //    foreach (var tapeRecord in records)
        //    {
        //        stream.Events.AddRange(this.DeserializeEvent(tapeRecord.Data));
        //        stream.Version = tapeRecord.Version;
        //    }
        //    return stream;
        //}

        public EventStoreStream(IAppendOnlyStoreStreamed appender, IEventRecordSerializer serializer)
        {
            this.appender = appender;
            this.serializer = serializer;
        }

        public async Task<EventStream> LoadEventStream(IIdentity id, long skip, long take)
        {
            var result = new EventStream();

            var records = await this.appender.ReadRecords(id, skip, take);

            while (!records.EndOfStream)
            {                
                var record = await records.ReadAsync();
                result.Events = await serializer.Read(record.Stream);
                result.Version = 0;
            }

            return result;
        }

        public Task<EventStream> LoadEventStream(long skip, long take)
        {
            return Master(skip,take);
        }

        public async Task AppendToStream(IIdentity identity, IEvent[] events, long expectedVersion)
        {
            var master = new MasterStreamRecord();
            var match = await this.appender.ReadRecords(identity, 0, expectedVersion + 1);

            if (match.Version != expectedVersion)
            {
                throw new Exception("Stream was updated");
            }

            var appender.Append(identity, events);

            serializer.Write(events, stream)

            
            master.Add()
        }
    }

    internal struct StringId : IIdentity
    {
        public string Id { get; set; }

        public StringId(string id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return this.Id;
        }
    }

    public interface IAppendOnlyStoreStreamed
    {
        Task<IEventstoreStreamRecord> ReadRecords(IIdentity id, long skip, long take);
    }
}