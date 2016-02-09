using TastyDomainDriven.Sample.Events;

namespace TastyDomainDriven.Sample.Properties
{
    public sealed class StateOfPerson : AggregateState, 
        IStateEvent<PersonSaidEvent>
    {
        public StateOfPerson()
        {
            this.LastMessage = "";
        }

        public string LastMessage { get; private set; }

        public void When(PersonSaidEvent e)
        {
            this.LastMessage = e.Saying ?? "";
        }
    }
}