namespace TastyDomainDriven.Saga
{
	using System;

	[Obsolete("Not recommended saga implementation")]
    public interface ISagaRoot : IAggregate
    {
        IBus Bus { get; set; }
    }
}