namespace TastyDomainDriven.AggregateService
{
	using System;
	using System.Linq;

	public class UpdateAggregate<TAggregateRoot> where TAggregateRoot : IAggregate, new()
	{
		private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(UpdateAggregate<TAggregateRoot>));

		public void Execute<TIdent>(IEventStore eventStorage, TIdent id, Action<TAggregateRoot> execute) where TIdent : IIdentity
		{
			if (eventStorage == null)
			{
				throw new ArgumentNullException("eventStorage");
			}

			if (id == null || id.Equals(default(TIdent)))
			{
				throw new ArgumentException("id is null or default value", "id");
			}

			// Load event stream from the store
			var stream = eventStorage.LoadEventStream(id);

			// create new Customer aggregate from the history
			var aggregate = new TAggregateRoot();
			aggregate.LoadsFromHistory(stream.Events);

			// execute delegated action
			Logger.DebugFormat("Executing Update on aggregate {0}", aggregate);
			execute(aggregate);

			// append resulting changes to the stream, expect the same version loaded before applying ddd logic
			if (aggregate.Changes.Any())
			{
				Logger.DebugFormat("Saving {0} uncommited events on aggregate {1}", aggregate.Changes.Count, aggregate);
				eventStorage.AppendToStream(id, stream.Version, aggregate.Changes);
			}

			Logger.DebugFormat("Finished Update on {0}", aggregate);
		}
	}
}