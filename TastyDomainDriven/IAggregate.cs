namespace TastyDomainDriven
{
    using System.Collections.Generic;

    public interface IAggregate
    {
        void LoadsFromHistory(IEnumerable<IEvent> events);

        List<IEvent> Changes { get; }

        //IIdentity Id { get; }
    }
}