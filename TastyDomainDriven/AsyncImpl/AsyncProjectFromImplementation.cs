using System;
using System.Linq;
using System.Threading.Tasks;
using TastyDomainDriven.PerformanceMeasurements;
using TastyDomainDriven.Projections;

namespace TastyDomainDriven.AsyncImpl
{
    /// <summary>
    /// Find all <![CDATA[IConsumesAsync<T>]]> on the projections and invoke consume.
    /// </summary>
    public sealed class AsyncProjectFromImplementation : IAsyncProjection, IConfigurableProfiling
    {
        private readonly EventRegisterAsync consumer;

        public AsyncProjectFromImplementation(params object[] projections)
        {
            this.consumer = new EventRegisterAsync(typeof(IConsumesAsync<>));

            ProjectionType = projections.FirstOrDefault()?.GetType();

            foreach (var p in projections)
            {
                consumer.Subscribe(p);
            }
        }

        public async Task Consume<T>(T @event) where T : IEvent
        {
            await consumer.Consume(@event);
        }

        public Type ProjectionType { get; }
    }
}