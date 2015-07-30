namespace TastyDomainDriven.Saga
{
    using System;

    [Obsolete("Not recommended saga implementation", true)]
    public class SagaRoot<T> : AggregateRoot<T>, ISagaRoot
        where T : AggregateState, new()
    {
        public IBus Bus { get; set; }
    }
}