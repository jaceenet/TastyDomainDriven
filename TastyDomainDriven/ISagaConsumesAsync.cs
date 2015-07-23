namespace TastyDomainDriven
{
	using System.Threading.Tasks;

	public interface ISagaConsumesAsync<in TEvent> where TEvent : IEvent
	{
		Task Consume(TEvent e);
	}
}