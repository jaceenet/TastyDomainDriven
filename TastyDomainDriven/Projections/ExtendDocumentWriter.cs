namespace TastyDomainDriven.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public static class ExtendDocumentWriter
    {
        public static TEntity AddOrUpdate<TKey, TEntity>(this IDocumentReaderWriter<TKey, TEntity> document, TKey key, Action<TEntity> update) where TEntity : new()
        {
            Func<TEntity> creator = () =>
            {
                TEntity item = new TEntity();
                update(item);
                return item;
            };
            Func<TEntity, TEntity> updator = entity =>
            {
                update(entity);
                return entity;
            };

            return document.AddOrUpdate(key, creator, updator);
        }

        public static IEnumerable<TEntity> Find<TKey, TEntity>(this IDocumentReaderWriter<TKey, TEntity> document, Func<TEntity, bool> predicate)
        {
            return document.GetAll().Where(predicate);
        }
    }
}