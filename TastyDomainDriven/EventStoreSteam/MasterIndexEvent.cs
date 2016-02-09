using System;

namespace TastyDomainDriven.EventStoreSteam
{
    public class MasterIndexEvent
    {
        public string AggregateId { get; set; }

        public long Version { get; set; }

        public Byte[] Checksum { get; set; }

        public Guid StreamId { get; set; }

        public long Position { get; }

        public long Length { get; }
    }
}