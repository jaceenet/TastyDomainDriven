namespace TastyDomainDriven.Projections
{
    using System;    
    using System.Collections.Generic;

    public interface IDocumentReaderWriter<in TKey, TEntity>
    {
        bool TryGet(TKey key, out TEntity entity);

        TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists);

        bool TryDelete(TKey key, TEntity rowkey);

        IEnumerable<TEntity> GetAll();
    }

    public interface IDocumentReaderWriter<TEntity> : IDocumentReaderWriter<IIdentity, TEntity>
    {
    }
}