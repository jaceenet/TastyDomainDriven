using System;
using TastyDomainDriven.Sample.Events;

namespace TastyDomainDriven.Sample.Properties
{
    public class PersonAggregate : AggregateRoot<StateOfPerson>
    {
        public void Say(PersonId id, string whatToSay, DateTime timestamp)
        {
            if (State.LastMessage.Equals(whatToSay))
            {
                throw new Exception("You can't repeat yourself inside or domain modelled");
            }

            this.Apply(new PersonSaidEvent
            {
                PersonId = id, 
                Saying = whatToSay,
                Timestamp = timestamp
            });
        }
    }
}