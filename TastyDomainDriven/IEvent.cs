namespace TastyDomainDriven
{
    using System;

    public interface IEvent
    {
        IIdentity AggregateId { get; }

        Guid EventId { get; }

        DateTime Timestamp { get; }

        //long Version { get; set; }
    }    
}