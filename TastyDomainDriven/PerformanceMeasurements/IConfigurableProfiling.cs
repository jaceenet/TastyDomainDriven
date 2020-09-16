using System;
using System.Collections.Generic;
using System.Text;

namespace TastyDomainDriven.PerformanceMeasurements
{
    /// <summary>
    /// Marked that projection is support profiling 
    /// </summary>
    public interface IConfigurableProfiling
    {
        Type ProjectionType { get; }
    }
}
