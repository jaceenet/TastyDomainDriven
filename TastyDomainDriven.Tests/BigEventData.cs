using System;

namespace TastyDomainDriven.Tests
{
    [Serializable]
    public class BigEventData : IEvent
    {
        public BigEventData()
        {
            
        }

        private static readonly Random Random = new Random();

        public static BigEventData Create(IIdentity id, int size)
        {
            byte[] buffer = new byte[size];
            Random.NextBytes(buffer);

            return new BigEventData()
            {
                EventId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                AggregateId = id,
                RandomData = buffer
            };
        }

        public IIdentity AggregateId { get; set; }
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }

        public byte[] RandomData { get; set; }
    }
}