namespace TastyDomainDriven
{
	using System.Threading.Tasks;

	public interface IConsumesAsync<in TEvent> where TEvent : IEvent
	{
		Task Consume(TEvent e);
	}
}