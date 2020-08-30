using System.Threading.Tasks;
using TastyDomainDriven.PerformanceMeasurements;

namespace TastyDomainDriven.AsyncImpl
{
    public class CompositeAsyncProjection : IAsyncProjection
    {
        private readonly IAsyncProjection[] projections;
        private readonly IPerformanceLogger performanceLogger;

        public CompositeAsyncProjection(params IAsyncProjection[] projections)
        {
            this.projections = projections;
        }

        public CompositeAsyncProjection(IPerformanceLogger performanceLogger, params IAsyncProjection[] projections)
        {
            this.projections = projections;
            this.performanceLogger = performanceLogger;
        }

        public async Task Consume<T>(T @event) where T : IEvent
        {
            ProjectionPerformance performance = null;
            if (performanceLogger != null)
            {
                performance = new ProjectionPerformance(@event);
            }

            foreach (IAsyncProjection projection in projections)
            {
                ProjectionMeasurement measurement = null;
                if (performance != null)
                {
                    measurement = new ProjectionMeasurement(projection.GetType());
                    measurement.Start();
                }

                await projection.Consume(@event);
                
                if (measurement != null)
                {
                    measurement.Stop();
                    performance.Add(measurement);
                }
            }

            if (performanceLogger != null)
            {
                await performanceLogger.TrackProjection(performance);
            }
        }
    }
}