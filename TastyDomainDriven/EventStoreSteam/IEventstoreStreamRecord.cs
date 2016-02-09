using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TastyDomainDriven.EventStoreSteam
{
    public interface IEventstoreStreamRecord
    {
        bool EndOfStream { get; }

        Stream Stream { get; };
            
        Task<IEventstoreStreamRecord> ReadAsync();

        long Version { get; }
    }

    public interface IAppenderRecord
    {
        IIdentity Identity { get; }

        Stream Stream { get; }

        long Version { get; set; }
    }

    public interface IAppenderRecords
    {
        bool EndOfRecords { get; }

        IAppenderRecord Next();
    }

    public class MasterStreamRecord : IAppenderRecords
    {
        LinkedList<IAppenderRecord> streams = new LinkedList<IAppenderRecord>();

        private LinkedListNode<IAppenderRecord> current = null;

        public bool EndOfRecords
        {
            get { return current.Next != null; }
        }

        public IAppenderRecord Next()
        {
            this.current = current.Next;
            return current.Value;
        }

        public Task Add(IIdentity identity, Stream stream, long version)
        {
            streams.AddLast(new LinkedListNode<IAppenderRecord>(new AppenderRecord(stream, version)))
        }
    }
}