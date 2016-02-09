using System.IO;

namespace TastyDomainDriven.EventStoreSteam
{
    public class AppenderRecord : IAppenderRecord
    {
        public AppenderRecord(IIdentity identity, Stream stream, long version)
        {
            Identity = identity;
            Stream = stream;
            Version = version;
        }

        public IIdentity Identity { get; set; }
        public Stream Stream { get; }
        public long Version { get; set; }
    }
}