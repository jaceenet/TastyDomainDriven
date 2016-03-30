namespace TastyDomainDriven.Dsl
{
    using System;

    public interface IEventVersionSerializer
    {
        int GetEventId { get; }
        Type EventType { get; }
    }
}
