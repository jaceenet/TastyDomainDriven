using System;
using System.Runtime.Serialization;

namespace TastyDomainDriven
{
    public class AppendOnlyTimeoutException : Exception
    {
        public long ExpectedStreamVersion { get; private set; }
        public long ActualStreamVersion { get; private set; }
        public string StreamName { get; private set; }

        protected AppendOnlyTimeoutException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }

        public AppendOnlyTimeoutException(long expectedVersion, long actualVersion, string name)
            : base(
                string.Format("Expected version {0} in stream '{1}' but timed out due to other appends in progress. (current version: {2})", expectedVersion, name, actualVersion))
        {
            this.StreamName = name;
            this.ExpectedStreamVersion = expectedVersion;
            this.ActualStreamVersion = actualVersion;
        }
    }
}