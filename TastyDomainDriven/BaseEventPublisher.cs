namespace TastyDomainDriven
{
	public abstract class BaseEventPublisher : IEventPublisher
	{
		public abstract void ConsumeByReadSide(IEvent[] appendedEvents);

		public abstract void ConsumeBySaga(IEvent[] appendedEvents);
	}
}