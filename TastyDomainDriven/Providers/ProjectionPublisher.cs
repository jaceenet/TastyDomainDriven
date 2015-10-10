using System;
using System.Linq;
using TastyDomainDriven.Bus;
using TastyDomainDriven.Projections;

namespace TastyDomainDriven.Providers
{
    /// <summary>
    /// Use projections from Providers
    /// </summary>
    public class ProjectionPublisher : IEventPublisher
    {
        private readonly ProjectionsProvider projections;
        private readonly IViewProvider viewProvider;
        private readonly Func<IBus> getBus;

        public ProjectionPublisher(ProjectionsProvider projections, IViewProvider viewProvider, Func<IBus> getBus)
        {
            this.projections = projections;
            this.viewProvider = viewProvider;
            this.getBus = getBus;
        }

        public void ConsumeByReadSide(IEvent[] appendedEvents)
        {
            var consumers = new EventRegister(typeof(IConsumesAsync<>));
            
            foreach (var instance in projections.Values.Where(x => x.type == ProjectionType.ReadFacade).Select(x => x.Create(this.viewProvider, new NoBus())))
            {
                consumers.Subscribe(instance);
            }

            foreach (var e in appendedEvents)
            {
                consumers.Consume(e);
            }
        }        

        public void ConsumeBySaga(IEvent[] appendedEvents)
        {
            var consumers = new EventRegister(typeof(IConsumesAsync<>));

            foreach (var instance in projections.Values.Where(x => x.type == ProjectionType.Saga).Select(x => x.Create(this.viewProvider, getBus())))
            {
                consumers.Subscribe(instance);
            }

            foreach (var e in appendedEvents)
            {
                consumers.Consume(e);
            }
        }
    }
}