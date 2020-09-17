namespace TastyDomainDriven.AggregateService
{
	using System;
	using System.Linq;

	public class CreateAggregateByUpdate<TAggregateRoot> where TAggregateRoot : IAggregate
	{
		private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(CreateAggregateByUpdate<TAggregateRoot>));

		public void Execute<TIdent, TResult, TIdentCreated>(IEventStore eventStorage, TIdent id, Func<TResult, TAggregateRoot> execute, TIdentCreated createId)
			where TIdent : IIdentity
			where TResult : IAggregate, new()
			where TIdentCreated : IIdentity
		{
			if (eventStorage == null)
			{
				throw new ArgumentNullException("eventStorage");
			}

			if (id == null || id.Equals(default(TIdent)))
			{
				throw new ArgumentException("id is null or default value", "id");
			}

			if (createId == null || createId.Equals(default(TIdentCreated)))
			{
				throw new ArgumentException("createid is null or default value", "createId");
			}

			EventStream stream = eventStorage.LoadEventStream(id);

			// create new Customer aggregate from the history
			var aggregate = new TResult();
			aggregate.LoadsFromHistory(stream.Events);

			// execute delegated action
			Logger.DebugFormat("Executing UpdateAndCreateAggregate on aggregate {0}", aggregate);
			TAggregateRoot res = execute(aggregate);

			if (aggregate.Changes.Any())
			{
				throw new Exception("You cannot modify more than one aggregate per command!");
			}

			if (res != null && res.Changes.Any())
			{
				Logger.DebugFormat("Saving {0} uncommited events on result aggregate {1}", res.Changes.Count, res);
				eventStorage.AppendToStream(createId, 0, res.Changes);
				Logger.DebugFormat("Finished Create on {0}", aggregate);
				return;
			}

			throw new InvalidOperationException("No aggregate created on execute");
		}
	}
}