using System.Threading.Tasks;
using TastyDomainDriven.Projections;

namespace TastyDomainDriven.AsyncImpl
{
    public sealed class AsyncProjectFromImplementation : IAsyncProjection
    {
        private readonly EventRegisterAsync consumer;

        public AsyncProjectFromImplementation(params object[] projections)
        {
            this.consumer = new EventRegisterAsync(typeof(IConsumesAsync<>));

            foreach (var p in projections)
            {
                consumer.Subscribe(p);
            }
        }

        public async Task Consume(IEvent @event)
        {
            await consumer.Consume(@event);
        }
    }
}