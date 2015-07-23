namespace TastyDomainDriven.Projections
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

	public class MemoryTableWriter<TEntity> : ITableReaderWriter<TEntity> where TEntity : ISingleKey
    {
        private readonly ConcurrentDictionary<string, TEntity> items = new ConcurrentDictionary<string, TEntity>();

	    async Task Initialise(ITableReaderWriter<TEntity> from)
	    {
	        var list = await @from.GetAll();

            foreach (var item in list)
            {
                var item1 = item;
                await this.InsertOrUpdate(item, entity => item1);
            }
	    }

	    public string TableName { get; set; }

        public int EventCount { get; set; }

        public Guid EventId { get; set; }

        public DateTime EventTimestamp { get; set; }

        public async Task<TEntity> Get(TEntity match)
        {
            TEntity item;
            if (this.items.TryGetValue(match.GetIndexKey(), out item))
            {
                return await Task.FromResult(item);
            }

            return await Task.FromResult(default(TEntity));
        }

        public async Task<List<TEntity>> GetAll()
        {
			return await Task.FromResult(this.items.Values.ToList());
        }

        public async Task InsertOrUpdate(TEntity add, Func<TEntity, TEntity> update)
        {
            Func<string, TEntity> addfun = s => add;
            Func<string, TEntity, TEntity> updatefun = (s, entity) => update(entity);

            this.items.AddOrUpdate(add.GetIndexKey(), addfun, updatefun);
        }

		public Task<TEntity> Remove(TEntity key)
		{
			TEntity item;
			this.items.TryRemove(key.GetIndexKey(), out item);
			return Task.FromResult(item);
		}
    }
}