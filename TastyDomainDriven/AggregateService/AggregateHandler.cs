using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.PerformanceMeasurements;

namespace TastyDomainDriven.AggregateService
{
    public abstract class AggregateHandler<TAggregateRoot> :
        ICommandHandler
        where TAggregateRoot : IAggregate
    {
        private readonly Dictionary<Type, Func<ICommand, ICommandExecutor>> executors = new Dictionary<Type, Func<ICommand, ICommandExecutor>>();

        protected AggregateHandler(IEventStoreAsync eventStore, IPerformanceLogger performanceLogger = null)
        {
            this.performanceLogger = performanceLogger;
            this.eventStore = eventStore;
        }

        protected void Register<T>(Func<T, Task> action) where T : class, ICommand
        {
            executors[typeof(T)] = cmd => new Executor<T>(cmd, action);
        }

        public ICommandExecutor GetExecutor(ICommand command)
        {
            Func<ICommand, ICommandExecutor> finder;
            return executors.TryGetValue(command.GetType(), out finder) ? finder.Invoke(command) : null;
        }

        protected readonly IEventStoreAsync eventStore;
        protected readonly IPerformanceLogger performanceLogger;

        protected virtual async Task Update<TIdent>(TIdent id, Action<TAggregateRoot> execute) where TIdent : IIdentity
        {
            await new UpdateAggregateAsync<TAggregateRoot>(performanceLogger).Execute(this.eventStore, id, execute);
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
        protected virtual async Task Create<TIdent, TResult, TCreateFromId>(TCreateFromId id, Func<TResult, TAggregateRoot> execute, TIdent createId)
            where TIdent : IIdentity
            where TCreateFromId : IIdentity
            where TResult : IAggregate
        {
            // Load event stream from the store
            await new CreateAggregateByUpdateAsync<TAggregateRoot>(performanceLogger).Execute(this.eventStore, id, execute, createId);
        }
    }
}