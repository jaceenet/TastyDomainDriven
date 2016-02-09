using System;
using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.Events
{
    [Serializable]
    public class PersonSaidEvent : IEvent
    {
        public PersonSaidEvent(PersonId id, string saying, DateTime timestamp, Guid? eventId = null)
        {
            this.Id = id;
            this.AggregateId = id;
            this.EventId = eventId ?? Guid.NewGuid();
            this.Timestamp = timestamp;
            this.Saying = saying;
        }

        public string Saying { get; }

        public PersonId Id { get; }
        public IIdentity AggregateId { get; }

        public Guid EventId { get; }
        public DateTime Timestamp { get; }
    }

    public class PSE
    {
        public Guid EventId { get; set; }

        public string Saying { get; set; }

        public int Aggregate { get; set; }

        public DateTime Timestamp { get; set; }

        public static PersonSaidEvent Convert(PSE pse)
        {
            return new PersonSaidEvent(new PersonId(pse.Aggregate), pse.Saying, pse.Timestamp, pse.EventId);
        }

        public static PSE Convert(PersonSaidEvent e)
        {
            return new PSE() { Aggregate = e.Id.id, Saying = e.Saying, Timestamp = e.Timestamp, EventId = e.EventId };
        }
    }
}