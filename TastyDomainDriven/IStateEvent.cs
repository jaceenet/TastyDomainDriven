namespace TastyDomainDriven
{
    public interface IStateEvent<in TEvent> where TEvent : IEvent
    {
        void When(TEvent e);
    }
}