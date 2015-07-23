namespace TastyDomainDriven
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;

	public static class TableWriterExtensions
	{
        /// <summary>
        /// This will always apply the update logic to the TEntity being saved ontop of what is stored.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="writer"></param>
        /// <param name="match"></param>
        /// <param name="update"></param>
		public static Task AddOrUpdate<TEntity>(this ITableWriter<TEntity> writer, TEntity match, Action<TEntity> update)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}

			var add = new Func<TEntity>(
				() =>
				{
					var e = match;
					update(e);
					return e;
				});

			Func<TEntity, TEntity> updatefun = entity =>
			{
				var item = entity;                
				update(item);
				return item;
			};

			return writer.InsertOrUpdate(add(), updatefun);
		}

        /// <summary>
        /// This will ignore the exisiting TEntity and replace it.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="writer"></param>
        /// <param name="match"></param>
        /// <param name="replace"></param>
        public static Task Replace<TEntity>(this ITableWriter<TEntity> writer, TEntity match, Action<TEntity> replace)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            var add = new Func<TEntity>(
                () =>
                {
                    var e = match;
                    replace(e);
                    return e;
                });

            Func<TEntity, TEntity> updatefun = entity =>
            {
                var item = match;
                replace(item);
                return item;
            };

            return writer.InsertOrUpdate(add(), updatefun);
        }

        /// <summary>
        /// Initialize from a source, every item is added by calling InsertOrUpdate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer"></param>
        /// <param name="copyFrom"></param>
        /// <returns></returns>
        public static async Task Initialize<T>(this ITableWriter<T> writer, ITableReader<T> copyFrom)
	    {
	        var list = await copyFrom.GetAll();

            foreach (var item in list)
            {
                var item1 = item;
                await writer.InsertOrUpdate(item, obj => item1);
            }
	    }
	}
    
}