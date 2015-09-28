using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyDomainDriven.Projections
{
	/// <summary>
	/// Read and Write to an underlying stream
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class SerializableTableReaderWriter<T> : ITableReaderWriter<T>
	{
		private readonly ITableReaderWriter<T> source;

		private bool isLoaded = false;

		protected SerializableTableReaderWriter(ITableReaderWriter<T> source)
		{
			this.source = source;
		}

		public async Task<T> Get(T match)
		{
			await Read();
			return await source.Get(match);
		}

		protected abstract Task Serialize(ITableReaderWriter<T> source);
		protected abstract Task Deserialize(ITableReaderWriter<T> source);

		private async Task Read()
		{
			if (!isLoaded)
			{
				await Deserialize(source);
				isLoaded = true;
			}
		}

		protected async Task InsertAll(IEnumerable<T> items)
		{
			foreach (T item in items)
			{
				await this.InsertOrUpdate(item, a => a);
			}
		}

		public async Task<List<T>> GetAll()
		{
			await Read();
			return await source.GetAll();
		}

		public async Task InsertOrUpdate(T add, Func<T, T> update)
		{
			await source.InsertOrUpdate(add, update);
			await Serialize(source);
		}

		public async Task<T> Remove(T key)
		{
			var item = await source.Remove(key);
			await Serialize(source);
			return item;
		}
	}
}