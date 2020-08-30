using System;
using System.Diagnostics;

namespace TastyDomainDriven.PerformanceMeasurements
{
    public class ProjectionMeasurement
    {
        private Stopwatch _stopwatch;
        private readonly Type _projectionType;

        public ProjectionMeasurement(Type projectionType)
        {
            _projectionType = projectionType;
        }

        public virtual void Start()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public virtual void Stop()
        {
            _stopwatch.Stop();
        }

        public TimeSpan Elapsed
        {
            get
            {
                return _stopwatch?.Elapsed ?? TimeSpan.Zero;
            }
        }

        public Type ProjectionType => _projectionType;
    }
}