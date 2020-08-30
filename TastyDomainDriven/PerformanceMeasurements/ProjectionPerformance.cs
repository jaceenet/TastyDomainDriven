using System.Collections.Generic;

namespace TastyDomainDriven.PerformanceMeasurements
{
    public class ProjectionPerformance
    {
        private List<ProjectionMeasurement> _measurements;
        private IEvent _event;

        public ProjectionPerformance(IEvent @event)
        {
            _event = @event;
            _measurements = new List<ProjectionMeasurement>();
        }

        public virtual void Add(ProjectionMeasurement measurement)
        {
            _measurements.Add(measurement);
        }

        public IEvent Event => _event;

        public List<ProjectionMeasurement> Measurements => _measurements;
    }
}