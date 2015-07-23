namespace TastyDomainDriven
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AggregateRoot<TState> : IAggregate where TState : AggregateState, new()
    {
        public readonly TState State = new TState();

        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(AggregateRoot<TState>));

        public AggregateRoot()
        {
            this.Changes = new List<IEvent>();
        }

        public void LoadsFromHistory(IEnumerable<IEvent> events)
        {
            var myevents = events as IEvent[] ?? events.ToArray();

            if (myevents.Any())
            {
                this.Id = myevents.First().AggregateId;

                Logger.DebugFormat("Loading {0} events from history on {1}", myevents.Count(), this.Id);                

                foreach (var e in myevents)
                {
                    this.Apply(e, false);
                }    
            }
            else
            {
                Logger.DebugFormat("Loaded empty aggregate on id {0}", this.Id);
            }
        }

        public void Apply(IEvent e)
        {
            this.Apply(e, true);
        }

        internal void Apply(IEvent e, bool persist)
        {
            if (e.EventId == Guid.Empty && persist)
            {
                throw new Exception("The EventId cannot be empty guid");
            }

			// pass each event to modify current in-memory state
			this.State.Mutate(e);

            // append event to change list for further persistence
            if (persist)
            {
                if (e.AggregateId == null)
                {
                    throw new NullReferenceException("AggregateId is null before saving event !" + e.GetType().FullName);
                }

                this.Changes.Add(e);
            }
        }

        public List<IEvent> Changes { get; private set; }

        public IIdentity Id { get; private set; }
    }
}