namespace TastyDomainDriven
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    public interface IEventSerializer
    {
        byte[] SerializeEvent(IEvent[] e);

        IEvent[] DeserializeEvent(byte[] data);
    }
}