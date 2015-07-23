namespace TastyDomainDriven
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITableWriter<TEntity>
    {
        //string TableName { get; }

        //int EventCount { get; }

        //Guid EventId { get; }

        //DateTime EventTimestamp { get; }

        Task InsertOrUpdate(TEntity add, Func<TEntity, TEntity> update);

		Task<TEntity>  Remove(TEntity key);        
    }
}