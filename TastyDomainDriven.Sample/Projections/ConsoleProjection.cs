using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;

namespace TastyDomainDriven.Sample.Projections
{
    internal sealed class ConsoleProjection : IAsyncProjection
    {
        private readonly IAsyncProjection asyncProjection;

        public ConsoleProjection(IAsyncProjection asyncProjection)
        {
            this.asyncProjection = asyncProjection;
        }

        public async Task Consume<T>(T @event) where T : IEvent
        {
            Console.WriteLine("Putting {0} into {1}", @event, asyncProjection.GetType());
            await this.asyncProjection.Consume(@event);
        }
    }
}