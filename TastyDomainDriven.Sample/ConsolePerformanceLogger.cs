using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TastyDomainDriven.PerformanceMeasurements;

namespace TastyDomainDriven.Sample
{
    class ConsolePerformanceLogger : IPerformanceLogger
    {
        public Task TrackProjection(ProjectionPerformance performance)
        {
            Console.WriteLine($"Tracking projections performance for event {performance.Event.GetType().FullName}");
            foreach (ProjectionMeasurement measurement in performance.Measurements)
            {
                Console.WriteLine($"Projection {measurement.ProjectionType.FullName}. Elapsed: {measurement.Elapsed}");
            }
            return Task.CompletedTask;
        }

        public Task TrackAggregate(AggregatePerformance performance)
        {
            Console.WriteLine($"Tracking aggregate performance for aggregate {performance.AggregateType.FullName}");
            Console.WriteLine($"Load events time: {performance.LoadEventsTime}");
            Console.WriteLine($"Restore state time: {performance.RestoreStateTime}");
            Console.WriteLine($"Execute aggregate time: {performance.ExecuteAggregateTime}");
            Console.WriteLine($"Save changes time: {performance.SaveChangesTime}");
            Console.WriteLine($"History events time: {performance.HistoryEventsCount}");
            Console.WriteLine($"New events time: {performance.NewEventsCount}");
            return Task.CompletedTask;
        }
    }
}
