using System;
using System.Diagnostics;

namespace TastyDomainDriven.PerformanceMeasurements
{
    public class AggregatePerformance
    {
        public TimeSpan LoadEventsTime { get; internal set; }

        public TimeSpan RestoreStateTime { get; internal set; }

        public TimeSpan ExecuteAggregateTime { get; internal set; }

        public TimeSpan SaveChangesTime { get; internal set; }

        public int HistoryEventsCount { get; internal set; }

        public int NewEventsCount { get; internal set; }

        public Type AggregateType { get; internal set; }
    }
}