namespace TastyDomainDriven.Saga
{
	using System;

	[Obsolete("Not recommended saga implementation", true)]
    public interface ISagaRoot : IAggregate
    {
        IBus Bus { get; set; }
    }
}