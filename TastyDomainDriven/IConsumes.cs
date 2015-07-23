namespace TastyDomainDriven
{
	public interface IConsumes<in TEvent> where TEvent : IEvent
    {
        void Consume(TEvent e);
    }
}