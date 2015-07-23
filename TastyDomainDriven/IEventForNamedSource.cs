namespace TastyDomainDriven
{
    public interface IEventForNamedSource : IEvent
    {
        IIdentity EventSource { get; set; }
    }
}