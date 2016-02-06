using System;
using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.Events
{
    [Serializable]
    public class PersonSaidEvent : IEvent
    {
        public PersonSaidEvent()
        {
            EventId = Guid.NewGuid();
        }

        public PersonId PersonId
        {
            get { return (PersonId) this.AggregateId; }
            set { this.AggregateId = value; }
        }

        public string Saying { get; set; }

        public IIdentity AggregateId { get; set; }
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}