using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TastyDomainDriven.EventStoreSteam
{
    public class MemoryStreamAppender : IAppendOnlyStoreStreamed
    {
        private List<IEventstoreStreamRecord> masterindex = new List<IEventstoreStreamRecord>();

        public Task<IEventstoreStreamRecord> ReadRecords(IIdentity id, long skip, long take)
        {
            throw new System.NotImplementedException();
        }

        private Task AppendEvent(IIdentity id, Stream stream)
        {
            masterindex.Add(new Record(id, stream));
        }

        private class Record : IEventstoreStreamRecord
        {
            private MemoryStream memStream;

            public Record(IIdentity id, Stream stream, long version)
            {
                
                this.memStream = new MemoryStream();
                this.Stream = stream;
                this.EndOfStream = stream == null;
                this.Version = version;
            }

            public bool EndOfStream { get; }

            public Stream Stream { get; }

            public Task<IEventstoreStreamRecord> ReadAsync()
            {
                return Task.FromResult());
            }

            public long Version { get; }
        }
    }    
}