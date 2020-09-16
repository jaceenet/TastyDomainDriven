using System;
using System.Threading.Tasks;

namespace TastyDomainDriven.PerformanceMeasurements
{
    public interface IPerformanceLogger
    {
        Task TrackProjection(ProjectionPerformance performance);

        Task TrackAggregate(AggregatePerformance performance);
    }
}
