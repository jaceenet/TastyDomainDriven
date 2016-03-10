using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public class CompositeAsyncProjection : IAsyncProjection
    {
        private readonly IAsyncProjection[] projections;

        public CompositeAsyncProjection(params IAsyncProjection[] projections)
        {
            this.projections = projections;
        }

        public async Task Consume<T>(T @event) where T : IEvent
        {
            foreach (var projection in projections)
            {
                await projection.Consume(@event);
            }
        }
    }
}