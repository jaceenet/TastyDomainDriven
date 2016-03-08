using System;

namespace TastyDomainDriven.AggregateService
{
    public class AggregateService<TAggregateRoot> : IApplicationService
        where TAggregateRoot : IAggregate, new()
    {
        private readonly IEventStore eventStorage;

        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(AggregateService<TAggregateRoot>));

        public AggregateService(IEventStore eventStorage)
        {
            if (eventStorage == null)
            {
                throw new ArgumentNullException("eventStorage");
            }

            this.eventStorage = eventStorage;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public void Execute(ICommand cmd)
        {
            // pass command to a specific method named when
            // that can handle the command
            ((dynamic)this).When((dynamic)cmd);
        }

        public void Update<TIdent>(TIdent id, Action<TAggregateRoot> execute) where TIdent : IIdentity
        {
			new UpdateAggregate<TAggregateRoot>().Execute(this.eventStorage, id, execute);
        }
        
        /// <summary>
        /// Create a aggreagate from another existing root. Only changes to the new aggregate is saved. 
        /// </summary>
        /// <typeparam name="TIdent">id type of created aggregate</typeparam>
        /// <typeparam name="TResult">Aggregate to save changes on</typeparam>
        /// <typeparam name="TCreateFromId">id to create from</typeparam>
        /// <param name="id">existing aggregate id</param>
        /// <param name="execute">invoke logic</param>
        /// <param name="createId">id to save changes to</param>
        public void Create<TIdent, TResult, TCreateFromId>(TCreateFromId id, Func<TResult,TAggregateRoot> execute, TIdent createId)
            where TIdent : IIdentity
            where TCreateFromId : IIdentity
            where TResult : IAggregate, new()
        {
            // Load event stream from the store
			new CreateAggregateByUpdate<TAggregateRoot>().Execute(this.eventStorage, id, execute, createId);
        }
    }
}