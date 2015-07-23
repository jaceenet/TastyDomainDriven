namespace TastyDomainDriven
{
	public interface ISagaConsumes<in TEvent> where TEvent : IEvent
	{
		void Consume(TEvent e);
	}
}