namespace TastyDomainDriven.AsyncImpl
{
    public class AsyncFrom
    {
        private readonly IEventStore es;
        private readonly object[] objectsHavingIAcceptCommandInterface;

        public AsyncFrom(IEventStore es, params IAsyncCommandDispatcher[] objectsHavingIAcceptCommandInterface)
        {
            this.es = es;
            this.objectsHavingIAcceptCommandInterface = objectsHavingIAcceptCommandInterface;
        }
    }
}