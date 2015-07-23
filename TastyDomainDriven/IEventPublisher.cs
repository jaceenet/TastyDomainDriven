namespace TastyDomainDriven
{
	public interface IEventPublisher
	{
		void ConsumeByReadSide(IEvent[] appendedEvents);

		void ConsumeBySaga(IEvent[] appendedEvents);
	}
}