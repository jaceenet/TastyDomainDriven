using System.Threading.Tasks;
using TastyDomainDriven.Projections;

namespace TastyDomainDriven.AsyncImpl
{
    /// <summary>
    /// Find all <![CDATA[ISagaConsumesAsync<T>]]> on the projections and invoke consume.
    /// </summary>
    public sealed class AsyncSagasFromImplementation : IAsyncProjection
    {
        private readonly EventRegisterAsync consumer;

        public AsyncSagasFromImplementation(params object[] projections)
        {
            this.consumer = new EventRegisterAsync(typeof(ISagaConsumesAsync<>));

            foreach (var p in projections)
            {
                consumer.Subscribe(p);
            }
        }

        public async Task Consume<T>(T @event) where T: IEvent
        {
            await consumer.Consume(@event);
        }
    }
}