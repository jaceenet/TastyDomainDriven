namespace TastyDomainDriven
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    public class OptimisticConcurrencyException : Exception
    {
        public long ActualVersion { get; private set; }
        public long ExpectedVersion { get; private set; }
        public IIdentity Id { get; private set; }
        public IList<IEvent> ActualEvents { get; private set; }
        public IList<IEvent> ConflictingEvents { get; private set; }

        OptimisticConcurrencyException(string message, long actualVersion, long expectedVersion, IIdentity id,
            IList<IEvent> serverEvents, IList<IEvent> conflictingEvents)
            : base(message)
        {
            this.ActualVersion = actualVersion;
            this.ExpectedVersion = expectedVersion;
            this.Id = id;
            this.ActualEvents = serverEvents;
            this.ConflictingEvents = conflictingEvents;
        }
        
        public static OptimisticConcurrencyException Create(long actual, long expected, IIdentity id,
            IList<IEvent> serverEvents, IList<IEvent> conflictEvents)
        {
            var message = string.Format("Expected v{0} but found v{1} in stream '{2}'", expected, actual, id);
            return new OptimisticConcurrencyException(message, actual, expected, id, serverEvents, conflictEvents);
        }

        protected OptimisticConcurrencyException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}