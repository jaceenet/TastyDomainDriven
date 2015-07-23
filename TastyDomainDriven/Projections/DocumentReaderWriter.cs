namespace TastyDomainDriven.Projections
{
    using System;
    using System.Collections.Generic;

    public class DocumentReaderWriter<TKey, TEntity> : IDocumentReaderWriter<TKey, TEntity>
    {
        private readonly Dictionary<TKey, TEntity> items = new Dictionary<TKey, TEntity>();

        public bool TryGet(TKey key, out TEntity entity)
        {
            if (this.items.ContainsKey(key))
            {
                entity = this.items[key];
                return true;
            }

            entity = default(TEntity);
            return false;
        }

        public TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            TEntity item = this.items.ContainsKey(key) ? this.items[key] : this.items[key] = addFactory();
            item = update(item);
            return item;
        }

        public bool TryDelete(TKey key, TEntity rowkey)
        {
            if (this.items.ContainsKey(key))
            {
                this.items.Remove(key);
                return true;
            }

            return false;
        }

        public IEnumerable<TEntity> GetAll()
        {
            return this.items.Values;
        }
    }

    public class DocumentReaderWriter<TEntity> : DocumentReaderWriter<IIdentity, TEntity>
    {
    }
}